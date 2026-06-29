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
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Authentication.Login
{
    public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;
        private readonly ITotpService _totpService;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly ApiSettings _apiSettings;

        public LoginCommandHandler(
          IUserRepository userRepository,
          IUnitOfWork unitOfWork,
          IIdentityService identityService,
          ITokenService tokenService,
          ITotpService totpService,
          IEmailService emailService,
          IOptions<ApiSettings> apiOptions,
          IHttpContextAccessor httpContextAccessor,
          IAuditService auditService)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            _tokenService = tokenService;
            _totpService = totpService;
            _auditService = auditService;
            _emailService = emailService;
            _apiSettings = apiOptions.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken ct)
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var emailResult = Email.Create(command.Email);
            if (emailResult.IsFailure)
                return Result<LoginResponse>.Failure(emailResult.Error);

            var user = await _userRepository.GetByEmailAsync(emailResult.Value, ct);
            if (user is null)
                return Result<LoginResponse>.Failure(UserErrors.InvalidCredentials);

            var availabilityResult = user.CheckAvailability();
            if (availabilityResult.IsFailure)
                return Result<LoginResponse>.Failure(availabilityResult.Error);

           
            if (!user.IsEmailConfirmed)
            {
                var confirmToken = await _identityService.GenerateEmailConfirmationTokenAsync(user.IdentityId);
                byte[] confirmTokenBytes = Encoding.UTF8.GetBytes(confirmToken);
                string safeconfirmToken = WebEncoders.Base64UrlEncode(confirmTokenBytes);
                var confirmationLink = $"{_apiSettings.FrontendUrl}/confirm-email?" +
                    $"userId={user.Id}&" +
                    $"token={safeconfirmToken}";

                await _emailService.SendActivationReminderEmailAsync(
                    user.Email.Value,
                    user.FullName.DisplayName,
                    confirmationLink);

                return Result<LoginResponse>.Failure(UserErrors.EmailNotConfirmed);
            }

            var passwordValid = await _identityService.CheckPasswordAsync(command.Email, command.Password, ct);

            if (!passwordValid)
            {
                user.RecordFailedLogin(ipAddress);
                _userRepository.Update(user);
                await _unitOfWork.CompleteAsync(ct);

                await _auditService.LogAsync(new AuditEntry(
                  ActorId: user.Id,
                  Action: AuditActions.UserLoginFailed,
                  EntityType: nameof(User),
                  EntityId: user.Id,
                  IpAddress: ipAddress,
                  Succeeded: false,
                  FailureReason: "Invalid password attempt."), ct);

                return Result<LoginResponse>.Failure(UserErrors.InvalidCredentials);
            }

            var loginResult = user.RecordSuccessfulLogin(ipAddress);
            _userRepository.Update(user);
            await _unitOfWork.CompleteAsync(ct);

           
            if (user.IsTwoFactorEnabled)
            {
                return Result<LoginResponse>.Success(new LoginResponse(
                  Token: null,
                  RefreshToken: null,
                  UserId: user.Id,
                  RequiresTwoFactor: true
                ));
            }

            
            var claims = new TokenClaims(
                UserId: user.IdentityId,  
                DomainUserId: user.Id,  
                Email: user.Email.Value,
                Username: user.Username.Value,
                Role: user.Role.ToString());

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            await _identityService.UpdateRefreshTokenAsync(user.IdentityId, refreshToken, ct);

            await _auditService.LogAsync(new AuditEntry(
              ActorId: user.Id,
              Action: AuditActions.UserLogin,
              EntityType: nameof(User),
              EntityId: user.Id,
              IpAddress: ipAddress,
              Succeeded: true), ct);

            return Result<LoginResponse>.Success(new LoginResponse(
              Token: accessToken,
              RefreshToken: refreshToken,
              UserId: user.Id,
              RequiresTwoFactor: false));
        }
    }
}