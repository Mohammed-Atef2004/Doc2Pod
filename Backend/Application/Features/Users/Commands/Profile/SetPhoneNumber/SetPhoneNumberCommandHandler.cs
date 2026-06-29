using Application.Features.Users.Commands.Authentication.VerifyTwoFactor;
using Application.Features.Users.Services;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.SharedKernel;
using Domain.Users;
using Domain.Users.Errors;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Profile.SetPhoneNumber
{
    public sealed class SetPhoneNumberCommandHandler : IRequestHandler<SetPhoneNumberCommand, Result>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;
        private readonly ITotpService _totpService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SetPhoneNumberCommandHandler(
            IUserRepository userRepository,
            IIdentityService identityService,
            IPasswordHasher passwordHasher,
            ITotpService totpService,
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _identityService = identityService;
            _totpService = totpService;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result> Handle(SetPhoneNumberCommand command, CancellationToken ct)
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user is null) 
                return Result.Failure(UserErrors.NotFound);

            var availabilityResult = user.CheckAvailability();
            if (availabilityResult.IsFailure)
                return availabilityResult;

            var isPasswordValid = await _identityService.CheckPasswordAsync(user.Email.Value, command.CurrentPassword, ct);
            if (!isPasswordValid)
            {
                user.RecordFailedLogin(ipAddress);
                _userRepository.Update(user);
                await _unitOfWork.CompleteAsync(ct);
                await _auditService.LogAsync(new AuditEntry(
                    ActorId: user.Id,
                    Action: AuditActions.UserSetPhoneNumber, 
                    EntityType: nameof(User),
                    EntityId: user.Id,
                    IpAddress: ipAddress,
                    Succeeded: false,
                    FailureReason: "Invalid password during phone number change."), ct);
                return Result.Failure(UserErrors.InvalidCredentials);
            }
            if (user.IsTwoFactorEnabled)
            {
                if (string.IsNullOrEmpty(command.TwoFactorCode) || !_totpService.Verify(user.TwoFactorSecret!, command.TwoFactorCode))
                {
                    await _auditService.LogAsync(new AuditEntry(
                        ActorId: user.Id,
                        Action: AuditActions.UserSetPhoneNumber,
                        EntityType: nameof(User),
                        EntityId: user.Id,
                        IpAddress: ipAddress,
                        Succeeded: false,
                        FailureReason: "2FA verification failed during phone number change."), ct);
                    return Result.Failure(UserErrors.SecurityErrors.InvalidTotpCode);
                }
            }

            if (user.PhoneNumber?.Value == command.NewPhoneNumber)
                return Result.Success();

            var result = user.SetPhoneNumber(command.NewPhoneNumber);
            if (result.IsFailure) 
                return result;
            _userRepository.Update(user);
            await _unitOfWork.CompleteAsync(ct);

            await _auditService.LogAsync(new AuditEntry(
                ActorId: user.Id,
                Action: AuditActions.UserSetPhoneNumber,
                EntityType: nameof(User),
                EntityId: user.Id,
                IpAddress: ipAddress,
                Succeeded: true), ct);

            return Result.Success();
        }
    }
}
