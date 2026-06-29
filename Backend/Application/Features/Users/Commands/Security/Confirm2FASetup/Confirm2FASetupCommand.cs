using Domain.SharedKernel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Security.Confirm2FASetup
{
    public record Confirm2FASetupCommand(
           Guid UserId,
           string TotpCode,
           string NewSecret
       ) : IRequest<Result>;
}
