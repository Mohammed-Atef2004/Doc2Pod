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

namespace Application.Features.Users.Commands.Authentication.ResetPassword
{
    public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITotpService _totpService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResetPasswordCommandHandler(
          IUserRepository userRepository,
          IIdentityService identityService,
          IPasswordHasher passwordHasher,
          IAuditService auditService,
          IUnitOfWork unitOfWork,
          ITotpService totpService,
          IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _identityService = identityService;
            _passwordHasher = passwordHasher;
            _auditService = auditService;
            _totpService = totpService;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken ct)
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user is null)
                return Result.Failure(UserErrors.NotFound);

            if (user.PasswordHashes.Any(h => _passwordHasher.VerifyPassword(command.NewPassword, h)))
            {
                return Result.Failure(UserErrors.SecurityErrors.PasswordAlreadyUsed);
            }
            var identityResult = await _identityService.ResetPasswordAsync(
              user.IdentityId,
              command.Token,
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
                await _unitOfWork.CompleteAsync(ct);
                return Result.Failure(identityResult.Error);
            }

            var newHash = _passwordHasher.HashPassword(command.NewPassword);
            user.UpdatePassword(newHash);
            await _unitOfWork.CompleteAsync(ct);

            await _auditService.LogAsync(new AuditEntry(
              ActorId: user.Id,
              Action: AuditActions.UserResetPassword,
              EntityType: "User",
              EntityId: user.Id,
              IpAddress: ipAddress,
              Succeeded: true
            ), ct);
            return Result.Success();
        }
    }
}