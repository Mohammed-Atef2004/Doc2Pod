
using  Domain.SharedKernel;

namespace  Domain.Users.Events
{
    public sealed record UserLoggedInSuccessfullyDomainEvent(
     Guid UserId,
     string IpAddress,
     DateTime LoginAt) : DomainEvent;
}
