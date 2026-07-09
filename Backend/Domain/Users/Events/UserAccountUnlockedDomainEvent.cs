using  Domain.SharedKernel;

namespace  Domain.Users.Events
{
    public sealed record UserAccountUnlockedDomainEvent(
    Guid UserId) : DomainEvent;
}
