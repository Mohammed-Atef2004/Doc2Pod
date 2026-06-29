using Domain.Enums;

namespace Application.Features.Podcasts.Query.GetAllPodcasts
{
    public record GetUserPodcastsResponse(
    Guid PodcastId,
    PodcastMode Mode,
    string PodcastName,
    DateTime CreatedAt,
    string? Topic,
    int? StartPage,
    int? EndPage);
}
