using Domain.SharedKernel;

namespace Domain.Documents.Errors
{
    public static class UploadDocumentError
    {
        public static readonly Error DocumentAlreadyExists = new("Document.AlreadyExists",
           "This PDF was uploaded before.");
    }
}
