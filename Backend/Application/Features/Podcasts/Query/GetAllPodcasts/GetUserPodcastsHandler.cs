using Application.Common.Extensions;
using Application.Common.Wrappers;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Interfaces.Repositories;
using Domain.SharedKernel;
using Domain.Users.Errors;
using MediatR;

namespace Application.Features.Podcasts.Query.GetAllPodcasts
{
    public class GetUserPodcastsHandler : IRequestHandler<GetUserPodcastsQuery, Result<PaginatedResult<GetUserPodcastsResponse>>>

    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        public GetUserPodcastsHandler(IUnitOfWork unitOfWork, IMapper mapper, IUserContext userContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userContext = userContext;
        }

        public async Task<Result<PaginatedResult<GetUserPodcastsResponse>>> Handle(GetUserPodcastsQuery request, CancellationToken cancellationToken)
        {

            var userid = _userContext.UserId;
            if (userid == null)
            {
                return Result<PaginatedResult<GetUserPodcastsResponse>>.Failure(UserErrors.InvalidUserId);

            }

            var query = _unitOfWork.Podcast.GetQueryablePodcastByUserId(userid.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(x =>
                    x.Document.FileName.Contains(request.SearchTerm));
            }

            if (!string.IsNullOrEmpty(request.SortBy))
            {
                switch (request.SortBy.ToLower())
                {
                    case "date":
                        query = request.SortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(v => v.CreatedAt)
                            : query.OrderBy(v => v.CreatedAt);
                        break;

                    default:
                        query = query.OrderBy(v => v.Id);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(v => v.Id);
            }


            var result = await query
            .ProjectTo<GetUserPodcastsResponse>(_mapper.ConfigurationProvider)
              .ToPaginatedListAsync(request.PageNumber, request.PageSize);

            return Result<PaginatedResult<GetUserPodcastsResponse>>.Success(result);

        }
    }
}
