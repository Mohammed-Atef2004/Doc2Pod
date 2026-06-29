using Domain.SharedKernel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Profile.ChangePassword
{
    public record ChangePasswordCommand(
        Guid UserId,
        string CurrentPassword,
        string NewPassword,
        string? TwoFactorCode) : IRequest<Result>;
}
