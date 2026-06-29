using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Authentication.VerifyTwoFactor
{
    public sealed record LoginResponse(
    string? Token,
    string? RefreshToken, 
    Guid UserId,
    bool RequiresTwoFactor
);
}

