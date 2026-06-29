using  Domain.SharedKernel;
namespace  Domain.Users.Events
{
    public sealed record UserPasswordChangedDomainEvent(
    Guid UserId,
    DateTime ChangedAt) : DomainEvent;
}
