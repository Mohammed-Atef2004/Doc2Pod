using  Domain.SharedKernel;
using  Domain.Users.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static  Domain.Users.Errors.UserErrors;

namespace  Domain.Users.Rules
{
    public sealed class AccountLockoutRule : IBusinessRule
    {
        public const int MaxFailedAttempts = 5;
        private readonly int _failedAttempts;

        public AccountLockoutRule(int failedAttempts)
            => _failedAttempts = failedAttempts;

        public bool IsBroken() => _failedAttempts >= MaxFailedAttempts;
        public Error Error => SecurityErrors.AccountLocked;
    }
}
