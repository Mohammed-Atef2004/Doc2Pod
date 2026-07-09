using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs.RealTime
{
    public class PodcastNotificationService : IPodcastNotificationService
    {
        private readonly IHubContext<PodcastHub> _hubContext;

        public PodcastNotificationService(IHubContext<PodcastHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyStatusChanged(Guid userId, Guid podcastId, string newStatus)
        {

            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveStatusUpdate", new
            {
                podcastId = podcastId.ToString(),
                status = newStatus
            });
        }
    }
}
