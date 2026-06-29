using Domain.Enums;
using FluentValidation;

namespace Application.Features.Podcasts.Commands.GeneratePodcast
{
    public class GeneratePodcastValidator
     : AbstractValidator<GeneratePodcastCommand>
    {
        public GeneratePodcastValidator()
        {
            RuleFor(x => x.DocumentId)
                .NotEmpty().WithMessage("Document Id is required");

            RuleFor(x => x.Mode)
                .NotEmpty().WithMessage("Mode is required")
                .Must(mode => Enum.IsDefined(typeof(PodcastMode), mode))
                .WithMessage("Invalid podcast mode");
        }
    }
}
