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

namespace Application.Features.Users.Commands.Authentication.VerifyTwoFactorLogin
{
    public sealed class VerifyTwoFactorLoginCommandHandler
                : IRequestHandler<VerifyTwoFactorLoginCommand, Result<VerifyTwoFactorLoginResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITotpService _totpService;
        private readonly ITokenService _tokenService;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VerifyTwoFactorLoginCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ITotpService totpService,
            ITokenService tokenService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _totpService = totpService;
            _tokenService = tokenService;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<VerifyTwoFactorLoginResponse>> Handle(
            VerifyTwoFactorLoginCommand command,
            CancellationToken ct)
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user is null)
                return Result<VerifyTwoFactorLoginResponse>.Failure(UserErrors.NotFound);

            if (!_totpService.Verify(user.TwoFactorSecret!, command.TotpCode))
            {
                user.RecordFailedLogin(ipAddress);
                await _unitOfWork.CompleteAsync(ct);

                await _auditService.LogAsync(new AuditEntry(
                    ActorId: user.Id,
                    Action: AuditActions.UserLoginFailed,
                    EntityType: nameof(User),
                    EntityId: user.Id,
                    IpAddress: ipAddress,
                    Succeeded: false,
                    FailureReason: "Invalid TOTP code provided."), ct);

                return Result<VerifyTwoFactorLoginResponse>.Failure(UserErrors.SecurityErrors.InvalidTotpCode);
            }

            user.RecordSuccessfulLogin(ipAddress);
            _userRepository.Update(user);
            await _unitOfWork.CompleteAsync(ct);

            var claims = new TokenClaims(
                DomainUserId: user.Id,
                UserId: user.IdentityId,
                Email: user.Email.Value,
                Username: user.Username.Value,
                Role: user.Role.ToString());

            var token = _tokenService.GenerateAccessToken(claims);

            await _auditService.LogAsync(new AuditEntry(
                ActorId: user.Id,
                Action: AuditActions.UserLogin,
                EntityType: nameof(User),
                EntityId: user.Id,
                IpAddress: ipAddress,
                Succeeded: true), ct);

            return Result<VerifyTwoFactorLoginResponse>.Success(
                new VerifyTwoFactorLoginResponse(token, user.Id));
        }
    }
}
