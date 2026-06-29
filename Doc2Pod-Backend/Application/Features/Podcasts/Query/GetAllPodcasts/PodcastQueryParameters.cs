namespace Application.Features.Podcasts.Query.GetAllPodcasts
{
    public class PodcastQueryParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "Date";
        public string SortDirection { get; set; } = "desc";
        public string SearchTerm { get; set; } = "";
    }
}
