
using Application.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.SharedKernel;
using Domain.Users.Errors;
using MediatR;

namespace Application.Features.Podcasts.Query.GetPodcastStatus
{
    public class GetPodcastStatusHandler
    : IRequestHandler<GetPodcastStatusQuery, Result<PodcastStatusResponse>>
    {
        private readonly IPodcastRepository _podcastRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;

        public GetPodcastStatusHandler(
            IPodcastRepository podcastRepository,
            IUnitOfWork unitOfWork,
            IUserContext userContext)
        {
            _podcastRepository = podcastRepository;
            _unitOfWork = unitOfWork;
            _userContext = userContext;
        }

        public async Task<Result<PodcastStatusResponse>> Handle(
            GetPodcastStatusQuery request,
            CancellationToken cancellationToken)
        {

            var userid = _userContext.UserId;
            if (userid == null)
            {
                return Result<PodcastStatusResponse>.Failure(UserErrors.InvalidUserId);

            }
            var podcast = await _unitOfWork.Podcast.GetRunningPodcastByUserIdAsync(userid.Value);

            if (podcast is null)
            {
                return Result<PodcastStatusResponse>.Success(
                    new PodcastStatusResponse(
                        null,
                        "None"
                    )
                );
            }

            return Result<PodcastStatusResponse>.Success(
                new PodcastStatusResponse(
                    podcast.Id,
                    podcast.Status.ToString()
                )
            );
        }
    }
}
