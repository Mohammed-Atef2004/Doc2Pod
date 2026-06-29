using  Domain.SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  Domain.Users.Events
{
    public sealed record UserAccountLockedDomainEvent(
    Guid UserId,
    DateTime LockedUntil) : DomainEvent;
}
