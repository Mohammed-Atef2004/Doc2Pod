using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Security.Setup2FA
{
    public class Setup2FACommandValidator : AbstractValidator<Setup2FACommand>
    {
        public Setup2FACommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        }
    }
}
