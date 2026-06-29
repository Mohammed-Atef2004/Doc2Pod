using  Domain.SharedKernel;
namespace  Domain.Users.Events
{
    public sealed record UserTwoFactorDisabledDomainEvent(
     Guid UserId) : DomainEvent;
}
