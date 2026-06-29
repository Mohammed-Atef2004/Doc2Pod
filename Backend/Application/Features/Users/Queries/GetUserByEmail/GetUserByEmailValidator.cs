using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Queries.GetUserByEmail
{
    public class GetUserByEmailValidator : AbstractValidator<GetUserByEmailQuery>
    {
        public GetUserByEmailValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email address is required.")
                .EmailAddress().WithMessage("A valid email address is required.");
        }
    }
}
