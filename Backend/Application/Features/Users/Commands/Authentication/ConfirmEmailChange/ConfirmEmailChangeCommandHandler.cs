using Application.Features.Users.Services;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.SharedKernel;
using Domain.Users;
using Domain.Users.Errors;
using Domain.Users.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Users.Errors.UserErrors;

namespace Application.Features.Users.Commands.Authentication.ConfirmEmailChange
{
    public sealed class ConfirmEmailChangeCommandHandler : IRequestHandler<ConfirmEmailChangeCommand, Result<string>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ConfirmEmailChangeCommandHandler(
            IUserRepository userRepository,
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService)
        {
            _userRepository = userRepository;
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<string>> Handle(ConfirmEmailChangeCommand command, CancellationToken ct)
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user is null) 
                return Result<string>.Failure(UserErrors.NotFound);

            var identityResult = await _identityService.ConfirmEmailChangeAsync(
                user.IdentityId,
                command.NewEmail,
                command.Token,
                ct);

            if (identityResult.IsFailure)
                return Result<string>.Failure(identityResult.Error);
            var newMail = Email.Create(command.NewEmail).Value;

            var updateResult = user.UpdateEmail(newMail, isEmailTaken: false);
            if (updateResult.IsFailure)
                return Result<string>.Failure(updateResult.Error);

            _userRepository.Update(user);
            await _unitOfWork.CompleteAsync(ct);

            await _auditService.LogAsync(new AuditEntry(
                ActorId: user.Id,
                Action: AuditActions.UserChangeEmail,
                EntityType: nameof(User),
                EntityId: user.Id,
                IpAddress: ipAddress,
                Succeeded: true),ct);

            var claims = new TokenClaims(
            DomainUserId: user.Id,
            UserId: user.IdentityId,
            Email: user.Email.Value,
            Username: user.Username.Value,
            Role: user.Role.ToString());

            var token = _tokenService.GenerateAccessToken(claims);
            return Result<string>.Success(token);
        }
    }
}