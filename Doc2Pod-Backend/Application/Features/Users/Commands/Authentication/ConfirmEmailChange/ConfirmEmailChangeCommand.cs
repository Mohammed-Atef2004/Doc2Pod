using Domain.SharedKernel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Authentication.ConfirmEmailChange
{
    public sealed record ConfirmEmailChangeCommand(
        Guid UserId,
        string NewEmail,
        string Token) : IRequest<Result<string>>;
}
