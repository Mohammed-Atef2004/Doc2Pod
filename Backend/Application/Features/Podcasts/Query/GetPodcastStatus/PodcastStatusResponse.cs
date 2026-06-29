namespace Application.Features.Podcasts.Query.GetPodcastStatus
{
    public record PodcastStatusResponse(
    Guid? PodcastId,
    string Status);
}
