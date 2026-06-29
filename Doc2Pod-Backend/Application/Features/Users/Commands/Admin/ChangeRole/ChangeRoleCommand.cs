using Domain.SharedKernel;
using Domain.Users;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Admin.ChangeRole
{
    public sealed record ChangeRoleCommand(
    Guid TargetUserId,
    Guid ActorId,
    UserRole NewRole
) : IRequest<Result>;
}
