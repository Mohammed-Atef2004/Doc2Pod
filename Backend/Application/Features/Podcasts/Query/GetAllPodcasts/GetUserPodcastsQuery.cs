using Application.Common.Wrappers;
using Domain.SharedKernel;
using MediatR;

namespace Application.Features.Podcasts.Query.GetAllPodcasts
{
    public class GetUserPodcastsQuery : IRequest<Result<PaginatedResult<GetUserPodcastsResponse>>>
    {
        public string? SortBy { get; init; }
        public string? SortDirection { get; init; }
        public string? SearchTerm { get; init; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
