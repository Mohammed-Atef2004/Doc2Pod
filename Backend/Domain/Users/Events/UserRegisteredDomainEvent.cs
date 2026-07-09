using  Domain.SharedKernel;

namespace  Domain.Users.Events;


public sealed record UserRegisteredDomainEvent(
    Guid UserId,
    string Email,
    string FullName,
    string Username,
    UserRole Role) : DomainEvent;

