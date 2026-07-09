using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Profile.SetPhoneNumber
{
    public sealed class SetPhoneNumberCommandValidator : AbstractValidator<SetPhoneNumberCommand>
    {
        public SetPhoneNumberCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User id is required.");

            RuleFor(x => x.NewPhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+[1-9]\d{6,14}$").WithMessage("Phone number must start with '+' followed by 7 to 15 digits.")
            .MaximumLength(16).WithMessage("Phone number is too long.");

            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required to verify your identity.");


            RuleFor(x => x.TwoFactorCode)
                .Matches(@"^\d{6}$")
                .WithMessage("2FA code must be exactly 6 digits.")
                .When(x => !string.IsNullOrEmpty(x.TwoFactorCode));
        }
    }
}
