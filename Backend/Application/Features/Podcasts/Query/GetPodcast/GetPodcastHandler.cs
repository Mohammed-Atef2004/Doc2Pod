using Application.Interfaces;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Podcasts.Errors;
using Domain.SharedKernel;
using MediatR;

namespace Application.Features.Podcasts.Query.GetPodcast
{
    public class GetPodcastHandler
    {
        public class GetAudioStreamQueryHandler : IRequestHandler<GetPodcastQuery, Result<string>>
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IFileStorageService _storageService;
            private readonly HttpClient _httpClient;
            private readonly IUserContext _userContext;
            public GetAudioStreamQueryHandler(
                IUnitOfWork unitOfWork,
                IFileStorageService storageService,
                HttpClient httpClient,
                IUserContext userContext)
            {
                _unitOfWork = unitOfWork;
                _storageService = storageService;
                _httpClient = httpClient;
                _userContext = userContext;
            }

            public async Task<Result<string>> Handle(GetPodcastQuery request, CancellationToken cancellationToken)
            {
                var userid = _userContext.UserId;
                var podcast = await _unitOfWork.Podcast.GetByIdAsync(request.Id);


                if (podcast == null)
                    return Result<string>.Failure(GetPodcastErrors.NotFound);

                if (podcast.UserId != userid)
                    return Result<string>.Failure(GetPodcastErrors.Unauthorized);

                if (podcast.Status != PodcastStatus.Completed)
                    return Result<string>.Failure(GetPodcastErrors.NotReady);

                if (string.IsNullOrEmpty(podcast.AudioPath))
                    return Result<string>.Failure(GetPodcastErrors.MissingAudioPath);
                var signedUrl = await _storageService
                    .GetSignedUrlAsync("Podcasts", podcast.AudioPath);

                if (string.IsNullOrEmpty(signedUrl))
                    return Result<string>.Failure(GetPodcastErrors.StorageAccessFailed);

                return Result<string>.Success(signedUrl);
            }
        }
    }
}
