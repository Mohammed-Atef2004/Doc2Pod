using Domain.SharedKernel;

namespace Application.Interfaces
{
    public interface IPdfValidationService
    {
        public Result ValidatePageRange(
            int? startPage,
            int? endPage);
    }
}
