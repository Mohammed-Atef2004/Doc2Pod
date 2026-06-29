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

namespace Application.Features.Users.Commands.Authentication.ConfirmEmail
{
    public sealed class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Result>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ConfirmEmailCommandHandler(
            IUserRepository userRepository,
            IIdentityService identity,
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _identityService = identity;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result> Handle(ConfirmEmailCommand command, CancellationToken ct)
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user is null)
                return Result.Failure(UserErrors.NotFound);

            var identityResult = await _identityService.ConfirmEmailAsync(user.IdentityId, command.Token, ct);
            if (identityResult.IsFailure)
            {
                await _auditService.LogAsync(new AuditEntry(
                    ActorId: user.Id,
                    Action: AuditActions.UserConfirmEmail,
                    EntityType: nameof(User),
                    EntityId: user.Id,
                    IpAddress: ipAddress,
                    Succeeded: false,
                    FailureReason: identityResult.Error.Message), ct);
                return identityResult;
            }

            var result = user.ConfirmEmail();
            if (result.IsFailure)
                return result;
            _userRepository.Update(user);
            await _unitOfWork.CompleteAsync(ct);

            await _auditService.LogAsync(new AuditEntry(
                ActorId: user.Id,
                Action: AuditActions.UserConfirmEmail,
                EntityType: nameof(User),
                EntityId: user.Id,
                IpAddress: ipAddress,
                Succeeded: true), ct);

            return Result.Success();
        }
    }
}
