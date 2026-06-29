using Application.Features.Users.Commands.Authentication.VerifyTwoFactor;
using Domain.SharedKernel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Authentication.Login
{
    public sealed record LoginCommand(
        string Email,
        string Password) : IRequest<Result<LoginResponse>>;
}
