using FluentValidation;

namespace Application.Features.Podcasts.Query.GetPodcast
{
    internal class GetPodcastValidator : AbstractValidator<GetPodcastQuery>
    {
        public GetPodcastValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Podcast Id is required");
        }
    }
}
