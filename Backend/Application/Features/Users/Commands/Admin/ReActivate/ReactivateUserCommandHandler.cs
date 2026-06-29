using Application.Features.Users.Services;
using Domain.Interfaces.Repositories;
using Domain.SharedKernel;
using Domain.Users;
using Domain.Users.Errors;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Admin.ReActivate
{
    public sealed class ReactivateUserCommandHandler : IRequestHandler<ReactivateUserCommand, Result>
    {
        private readonly IUserRepository _users;
        private readonly IUnitOfWork _uow;
        private readonly IAuditService _audit;

        public ReactivateUserCommandHandler(IUserRepository users, IUnitOfWork uow, IAuditService audit)
        {
            _users = users;
            _uow = uow;
            _audit = audit;
        }

        public async Task<Result> Handle(ReactivateUserCommand command, CancellationToken ct)
        {
            var user = await _users.GetByIdAsync(command.UserId);
            if (user is null) return Result.Failure(UserErrors.NotFound);

            var result = user.Reactivate();
            if (result.IsFailure) return result;

            _users.Update(user);
            await _uow.CompleteAsync(ct);

            await _audit.LogAsync(new AuditEntry(
                ActorId: command.ActorId,
                Action: AuditActions.UserReactivated,
                EntityType: nameof(User),
                EntityId: user.Id,
                IpAddress: null,
                Succeeded: true), ct);

            return Result.Success();
        }
    }
}
