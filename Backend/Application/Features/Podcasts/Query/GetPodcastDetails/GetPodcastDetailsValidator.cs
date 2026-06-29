using FluentValidation;

namespace Application.Features.Podcasts.Query.GetPodcastDetails
{
    public class GetPodcastDetailsValidator : AbstractValidator<GetPodcastDetailsQuery>
    {
        public GetPodcastDetailsValidator()
        {
            RuleFor(x => x.podcastId)
                .NotEmpty().WithMessage("Podcast Id is required");
        }
    }
}
