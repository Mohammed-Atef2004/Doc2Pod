using Application.Interfaces;
using Domain.Podcasts.Events;
using MediatR;

namespace Application.Features.Podcasts.Events
{

    public class PodcastStatusChangedHandler
    : INotificationHandler<PodcastStatusChangedDomainEvent>
    {
        private readonly IPodcastNotificationService _notifier;

        public PodcastStatusChangedHandler(
            IPodcastNotificationService notifier)
        {
            _notifier = notifier;
        }

        public async Task Handle(
            PodcastStatusChangedDomainEvent notification,
            CancellationToken cancellationToken)
        {
            await _notifier.NotifyStatusChanged(
                notification.userId,
                notification.PodcastId,
                notification.NewStatus);
        }
    }
}
