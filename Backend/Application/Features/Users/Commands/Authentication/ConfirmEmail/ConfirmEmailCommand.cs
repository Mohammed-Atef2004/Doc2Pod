using Domain.SharedKernel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Authentication.ConfirmEmail
{
    public sealed record ConfirmEmailCommand(Guid UserId, string Token) : IRequest<Result>;
}
