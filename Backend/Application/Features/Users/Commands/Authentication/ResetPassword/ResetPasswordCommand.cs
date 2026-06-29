using Domain.SharedKernel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Authentication.ResetPassword
{
    public record ResetPasswordCommand(
        Guid UserId,
        string Token,
        string NewPassword,
        string? TwoFactorCode) : IRequest<Result>;
}
