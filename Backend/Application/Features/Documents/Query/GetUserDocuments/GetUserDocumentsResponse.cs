namespace Application.Features.Documents.Query.GetUserDocuments
{
    public record GetUserDocumentsResponse(
        Guid DocumentId,
        string DocumentName,
        DateTime CreatedAt
    );
}
