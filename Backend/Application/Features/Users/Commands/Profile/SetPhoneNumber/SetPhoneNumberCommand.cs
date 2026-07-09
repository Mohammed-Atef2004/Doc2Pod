using Domain.SharedKernel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Profile.SetPhoneNumber
{
    public record SetPhoneNumberCommand(
        Guid UserId,
        string NewPhoneNumber,
        string CurrentPassword,
        string? TwoFactorCode) : IRequest<Result>;
}
