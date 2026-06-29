using  Domain.SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  Domain.Users.Errors
{
    public static class RulesErrors
    {


        public static readonly Error UsernameMustBeUnique =
            new("User.Rules.UsernameMustBeUnique",
                "An account with this username already exists.");

        public static readonly Error EmailMustBeUnique =
            new("User.Rules.EmailMustBeUnique",
                "An active account already exists with this email address.");
    }
}
