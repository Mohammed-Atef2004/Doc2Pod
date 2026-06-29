using Application.Features.Users.Commands.Authentication.VerifyTwoFactor;
using Application.Features.Users.Services;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Settings;
using Domain.SharedKernel;
using Domain.Users;
using Domain.Users.Errors;
using Domain.Users.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static Domain.Users.Errors.UserErrors;
using static System.Net.WebRequestMethods;

namespace Application.Features.Users.Commands.Profile.ChangeEmail
{
    public sealed class ChangeEmailCommandHandler : IRequestHandler<ChangeEmailCommand, Result>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ITotpService _totpService;
        private readonly IAuditService _auditService;
        private readonly ApiSettings _apiSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChangeEmailCommandHandler(
          IUserRepository userRepository,
          IIdentityService identityService,
          IUnitOfWork unitOfWork,
          IEmailService emailService,
          ITotpService totpService,
          IAuditService auditService,
          IHttpContextAccessor httpContextAccessor,
          IOptions<ApiSettings> apiOptions)
        {
            _userRepository = userRepository;
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _totpService = totpService;
            _auditService = auditService;
            _apiSettings = apiOptions.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result> Handle(ChangeEmailCommand command, CancellationToken ct)
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user is null)
                return Result.Failure(UserErrors.NotFound);

            var availabilityResult = user.CheckAvailability();
            if (availabilityResult.IsFailure)
                return availabilityResult;

            var passwordValid = await _identityService.CheckPasswordAsync(user.Email.Value, command.CurrentPassword, ct);
            if (!passwordValid)
            {
                user.RecordFailedLogin(ipAddress);
                _userRepository.Update(user);
                await _unitOfWork.CompleteAsync(ct);

                await _auditService.LogAsync(new AuditEntry
                  (
                  ActorId: user.Id,
                  Action: AuditActions.UserEmailChangeFailed,
                  EntityType: nameof(User),
                  EntityId: user.Id,
                  IpAddress: ipAddress,
                  Succeeded: false,
                  "Invalid password during email change."), ct);

                return Result.Failure(UserErrors.InvalidCredentials);
            }
            if (user.IsTwoFactorEnabled)
            {
                if (string.IsNullOrEmpty(command.TwoFactorCode))
                    return Result.Failure(UserErrors.SecurityErrors.InvalidTwoFactorSecret);

                if (!_totpService.Verify(user.TwoFactorSecret!, command.TwoFactorCode))
                {
                    await _auditService.LogAsync(new AuditEntry(
                    ActorId: user.Id,
                    Action: AuditActions.UserEmailChangeFailed,
                    EntityType: nameof(User),
                    EntityId: user.Id,
                    IpAddress: ipAddress,
                    Succeeded: false,
                    FailureReason: "Two-factor authentication code verification failed during email change request."), ct);
                    return Result.Failure(UserErrors.SecurityErrors.InvalidTotpCode);
                }
            }


            var newEmailResult = Email.Create(command.NewEmail);
            if (newEmailResult.IsFailure)
                return Result.Failure(newEmailResult.Error);

            if (user.Email.Value == newEmailResult.Value.Value)
                return Result.Failure(UserErrors.SameEmail);

            var isEmailTaken = await _userRepository.ExistsByEmailAsync(newEmailResult.Value, ct);

            var checkResult = user.CanUpdateEmail(isEmailTaken);
            if (checkResult.IsFailure)
                return checkResult;

            var token = await _identityService.GenerateChangeEmailTokenAsync(user.IdentityId, command.NewEmail, ct);
            byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
            string safeToken = WebEncoders.Base64UrlEncode(tokenBytes);
            string safeEmail = WebUtility.UrlEncode(command.NewEmail);
            var confirmationLink = $"{_apiSettings.FrontendUrl}/email-update-confirm?" +
                       $"userId={user.Id}&" +
                       $"newEmail={safeEmail}&" +
                       $"token={safeToken}";
            var warningResult =await _emailService.SendEmailChangeWarningAsync
              (user.Email.Value,
              user.FullName.DisplayName,
              ct);
            if (warningResult.IsFailure)
            {
                return Result.Failure(warningResult.Error);
            }
            var changeMailResult =await _emailService.SendEmailChangeConfirmationAsync
              (command.NewEmail
              , user.FullName.DisplayName,
              confirmationLink, ct);
            if (changeMailResult.IsFailure)
            {
                return Result.Failure(changeMailResult.Error);
            }
            return Result.Success();
        }
    }
}