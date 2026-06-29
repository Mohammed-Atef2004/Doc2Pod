namespace Application.Features.Podcasts.DTOs.Requests
{
    public class GeneratePodcastRequest
    {
        public string FileKey { get; set; }
        public int Mode { get; set; }
        public string? Topic { get; set; }
        public int? StartPage { get; set; }
        public int? EndPage { get; set; }
    }
}
