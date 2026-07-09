using Domain.SharedKernel;
using MediatR;

namespace Application.Features.Podcasts.Query.GetPodcast
{
    public record GetPodcastQuery(Guid Id) : IRequest<Result<string>>;
}
