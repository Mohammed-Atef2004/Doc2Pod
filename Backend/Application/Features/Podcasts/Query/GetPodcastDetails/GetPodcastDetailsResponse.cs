using Domain.Enums;

namespace Application.Features.Podcasts.Query.GetPodcastDetails
{
    public record GetPodcastDetailsResponse(
    PodcastMode Mode,
    string PodcastName,
    DateTime CreatedAt,
    string? Topic,
    int? StartPage,
    int? EndPage);
}
