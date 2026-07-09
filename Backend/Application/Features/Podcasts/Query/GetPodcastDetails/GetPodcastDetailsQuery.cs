using Domain.SharedKernel;
using MediatR;

namespace Application.Features.Podcasts.Query.GetPodcastDetails
{
    public record GetPodcastDetailsQuery(Guid podcastId) : IRequest<Result<GetPodcastDetailsResponse>>;

}
