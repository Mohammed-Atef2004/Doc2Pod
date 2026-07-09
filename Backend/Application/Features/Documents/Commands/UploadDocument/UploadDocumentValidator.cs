using FluentValidation;

namespace Application.Features.Documents.Commands.UploadDocument
{
    internal class UploadDocumentValidator :
    AbstractValidator<UploadDocumentCommand>
    {
        public UploadDocumentValidator()
        {
            RuleFor(x => x.File)
                .NotEmpty().WithMessage("File is required");

        }
    }
}
