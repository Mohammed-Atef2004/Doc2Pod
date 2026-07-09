namespace Application.Interfaces
{
    public interface IPodcastNotificationService
    {
        Task NotifyStatusChanged(Guid userId, Guid podcastId, string status);
    }
}
