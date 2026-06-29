using Domain.SharedKernel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Authentication.Register
{
    public sealed record RegisterUserCommand(
    string Email,
    string Username,
    string Password,
    string FirstName,
    string LastName) : IRequest<Result<Guid>>;
}

