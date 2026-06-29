using  Domain.SharedKernel;
namespace  Domain.Users.Events
{
    public sealed record UserTwoFactorEnabledDomainEvent(
    Guid UserId) : DomainEvent;
}
