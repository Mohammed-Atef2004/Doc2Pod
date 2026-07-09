using Domain.SharedKernel;
using MediatR;

namespace Application.Features.Podcasts.Query.GetPodcastStatus
{
    public record GetPodcastStatusQuery() : IRequest<Result<PodcastStatusResponse>>;
}
