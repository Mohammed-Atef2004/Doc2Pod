using Application.Common.Wrappers;
using Domain.SharedKernel;
using MediatR;

namespace Application.Features.Documents.Query.GetUserDocuments
{
    public record GetUserDocumentsQuery : IRequest<Result<PaginatedResult<GetUserDocumentsResponse>>>
    {
        public string? SortBy { get; init; }
        public string? SortDirection { get; init; }
        public string? SearchTerm { get; init; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
