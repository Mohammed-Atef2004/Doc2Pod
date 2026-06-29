using Domain.SharedKernel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Profile.ChangeEmail
{
    public sealed record ChangeEmailCommand(
        Guid UserId,
        string NewEmail,
        string CurrentPassword,
        string? TwoFactorCode) : IRequest<Result>;
}
