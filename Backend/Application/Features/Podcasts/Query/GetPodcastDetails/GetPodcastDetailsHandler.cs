using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Podcasts.Errors;
using Domain.SharedKernel;
using MediatR;


namespace Application.Features.Podcasts.Query.GetPodcastDetails
{
    public class GetPodcastDetailsHandler : IRequestHandler<GetPodcastDetailsQuery, Result<GetPodcastDetailsResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;


        public GetPodcastDetailsHandler(
            IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<GetPodcastDetailsResponse>> Handle(GetPodcastDetailsQuery request, CancellationToken cancellationToken)
        {
            var podcast = await _unitOfWork.Podcast.GetByIdWithDocumentAsync(request.podcastId);

            if (podcast == null)
                return Result<GetPodcastDetailsResponse>.Failure(GetPodcastErrors.NotFound);

            Console.WriteLine($"Topic = {podcast.Topic}");
            Console.WriteLine($"StartPage = {podcast.StartPage}");
            Console.WriteLine($"EndPage = {podcast.EndPage}");
            var mapPodcast = _mapper.Map<GetPodcastDetailsResponse>(podcast);
            return Result<GetPodcastDetailsResponse>.Success(mapPodcast);
        }
    }
}
