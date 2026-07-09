using Application.Interfaces;
using Domain.Podcasts.Errors;
using Domain.SharedKernel;

namespace Infrastructure.Services
{
    public class PdfValidationService : IPdfValidationService
    {
        public Result ValidatePageRange(int? startPage, int? endPage)
        {
            if (!startPage.HasValue || !endPage.HasValue)
            {
                return Result.Failure(
                    GeneratePodcastErrors.InvalidPageRange);
            }

            if (startPage <= 0)
            {
                return Result.Failure(
                    GeneratePodcastErrors.InvalidStartPage);
            }

            if (endPage <= 0)
            {
                return Result.Failure(
                    GeneratePodcastErrors.InvalidEndPage);
            }

            if (startPage > endPage)
            {
                return Result.Failure(
                    GeneratePodcastErrors.InvalidPageRange);
            }
            return Result.Success();
        }

    }
}
