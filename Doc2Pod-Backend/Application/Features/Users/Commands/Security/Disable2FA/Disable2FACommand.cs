using Domain.SharedKernel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Security.Disable2FA
{
    public sealed record Disable2FACommand(Guid UserId) : IRequest<Result>;

}
