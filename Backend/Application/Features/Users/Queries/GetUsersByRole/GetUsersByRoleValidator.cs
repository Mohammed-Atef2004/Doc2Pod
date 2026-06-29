using Domain.Users;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Queries.GetUsersByRole
{
    public class GetUsersByRoleValidator : AbstractValidator<GetUsersByRoleQuery>
    {
        public GetUsersByRoleValidator()
        {
            RuleFor(x => x.Role)
                .IsEnumName(typeof(UserRole), caseSensitive: false)
                .WithMessage("The provided role is not a valid system role.");
        }
    }
}
