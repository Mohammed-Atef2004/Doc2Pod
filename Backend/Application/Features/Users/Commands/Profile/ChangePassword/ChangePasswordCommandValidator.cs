using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Profile.ChangePassword
{
    public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required to verify your identity.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
                .Matches(@"[\!\?\*\.\@\#\$\%\^\&\(\)]").WithMessage("Password must contain at least one special character (!?*@#$...).")
                .NotEqual(x => x.CurrentPassword).WithMessage("New password cannot be the same as the current password.");

            RuleFor(x => x.TwoFactorCode)
                .Matches(@"^\d{6}$").WithMessage("2FA code must be exactly 6 digits.")
                .When(x => !string.IsNullOrEmpty(x.TwoFactorCode));
        }
    }
}
