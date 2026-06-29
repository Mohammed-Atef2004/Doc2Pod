using Application.Interfaces;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Podcasts.Errors;
using Domain.SharedKernel;
using Domain.Users.Errors;
using Hangfire;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Features.Podcasts.Commands.GeneratePodcast
{
    public class GeneratePodcastHandler : IRequestHandler<GeneratePodcastCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPythonRagService _ragService;
        private readonly IPdfValidationService _pdfValidationService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IUserContext _userContext;

        public GeneratePodcastHandler(
            IPythonRagService ragService,
            IUnitOfWork unitOfWork,
            IServiceScopeFactory scopeFactory,
            IPdfValidationService pdfValidationService,
            IUserContext userContext)
        {
            _ragService = ragService;
            _unitOfWork = unitOfWork;
            _scopeFactory = scopeFactory;
            _pdfValidationService = pdfValidationService;
            _userContext = userContext;
        }

        public async Task<Result<Guid>> Handle(GeneratePodcastCommand command, CancellationToken cancellationToken)
        {
            var userid = _userContext.UserId;
            if (userid == null)
            {
                return Result<Guid>.Failure(UserErrors.InvalidUserId);

            }

            var initialDocument = await _unitOfWork.Document.GetByIdAsync(command.DocumentId);

            if (initialDocument == null)
                return Result<Guid>.Failure(GeneratePodcastErrors.DocumentNotFound);

            if (command.Mode == PodcastMode.Full)
            {
                var alreadyExists = await _unitOfWork.Podcast.ExistsAsync(p =>
                    p.DocumentId == command.DocumentId &&
                    (int)p.Mode == 3 && p.Status == PodcastStatus.Completed);

                if (alreadyExists)
                {
                    return Result<Guid>.Failure(GeneratePodcastErrors.FullPodcastAlreadyExists);
                }
            }
            if (command.Mode == PodcastMode.PageRange)
            {
                var validationResult =
                 _pdfValidationService.ValidatePageRange(
                        command.StartPage,
                        command.EndPage);

                if (validationResult.IsFailure)
                {
                    return Result<Guid>.Failure(
                        validationResult.Error);
                }
                if (command.EndPage > initialDocument.PageCount)
                    return Result<Guid>.Failure(GeneratePodcastErrors.InvalidEndPageExceedsTotal(initialDocument.PageCount));
            }
            var runningPodcast = await _unitOfWork.Podcast.GetRunningPodcastByUserIdAsync(userid.Value);
            if (runningPodcast != null)
            {
                return Result<Guid>.Failure(GeneratePodcastErrors.PodcastAlreadyRunning);
            }
            var podcast = initialDocument.AddPodcast
            (
                userid.Value,
                command.Mode,
                command.Topic,
                command.StartPage,
                command.EndPage,
                PodcastStatus.Pending
            );

            await _unitOfWork.Podcast.AddAsync(podcast);
            await _unitOfWork.CompleteAsync(cancellationToken);

            BackgroundJob.Enqueue<IPodcastService>(s =>
                s.ProcessPodcastGenerationAsync(podcast.Id, initialDocument.FilePath, command));

            return Result<Guid>.Success(podcast.Id);

        }
    }
}
