using Amazon.Runtime.Internal;
using Domain.SharedKernel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Admin.Deactivate
{
    public sealed record DeactivateUserCommand(
     Guid UserId,
     Guid ActorId,
     string Reason
 ) : IRequest<Result>;
}
