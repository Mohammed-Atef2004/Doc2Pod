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

namespace Application.Features.Users.Commands.Profile.ChangePassword
{
    public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;
        private readonly ITotpService _totpService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPasswordHasher _passwordHasher;


        public ChangePasswordCommandHandler(
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
            _passwordHasher = passwordHasher;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken ct)
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
                    Action: AuditActions.UserChangePassword,
                    EntityType: nameof(User),
                    EntityId: user.Id,
                    IpAddress: ipAddress,
                    Succeeded: false,
                    FailureReason: "Invalid current password."), ct);
                return Result.Failure(UserErrors.InvalidCredentials);
            }

            if (user.IsTwoFactorEnabled)
            {
                if (string.IsNullOrEmpty(command.TwoFactorCode) || !_totpService.Verify(user.TwoFactorSecret!, command.TwoFactorCode))
                {
                    await _auditService.LogAsync(new AuditEntry(
                        ActorId: user.Id,
                        Action: AuditActions.UserChangePassword,
                        EntityType: nameof(User),
                        EntityId: user.Id,
                        IpAddress: ipAddress,
                        Succeeded: false,
                        FailureReason: "Invalid 2FA code during password change."), ct);
                    return Result.Failure(UserErrors.SecurityErrors.InvalidTotpCode);
                }
            }

            if (user.PasswordHashes.Any(h => _passwordHasher.VerifyPassword(command.NewPassword, h)))
            {
                return Result.Failure(UserErrors.SecurityErrors.PasswordAlreadyUsed);
            }
            var identityResult = await _identityService.ChangePasswordAsync(
                user.IdentityId,command.CurrentPassword,
                command.NewPassword,
                ct);

            if (identityResult.IsFailure)
            {
                await _auditService.LogAsync(new AuditEntry(
                    ActorId: user.Id,
                    Action: AuditActions.UserResetPassword,
                    EntityType: "User",
                    EntityId: user.Id,
                    IpAddress: ipAddress,
                    Succeeded: false,
                    FailureReason: identityResult.Error.Message
                ), ct);
                return Result.Failure(identityResult.Error);
            }

            var newHash = _passwordHasher.HashPassword(command.NewPassword);

            _userRepository.Update(user);
            await _unitOfWork.CompleteAsync(ct);

            await _auditService.LogAsync(new AuditEntry(
                ActorId: user.Id,
                Action: AuditActions.UserChangePassword,
                EntityType: nameof(User),
                EntityId: user.Id,
                IpAddress: ipAddress,
                Succeeded: true), ct);

            return Result.Success();
        }
    }
}
