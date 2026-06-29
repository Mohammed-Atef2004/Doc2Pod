using Domain.SharedKernel;

namespace Domain.Podcasts.Events
{
    public sealed record PodcastStatusChangedDomainEvent(
        Guid userId,
    Guid PodcastId,
    string NewStatus) : DomainEvent;
}
