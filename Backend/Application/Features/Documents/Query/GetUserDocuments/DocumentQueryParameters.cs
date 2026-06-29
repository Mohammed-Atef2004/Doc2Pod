namespace Application.Features.Documents.Query.GetUserDocuments
{
    public class DocumentQueryParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "Date";
        public string SortDirection { get; set; } = "desc";
        public string SearchTerm { get; set; } = "";
    }
}
