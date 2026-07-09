namespace Application.Features.Podcasts.DTOs.Responses
{
    public class PodcastGenerationStatusResponse
    {
        public string Status { get; set; }
        public string? ScriptPath { get; set; }
        public string? AudioPath { get; set; }
        public string? Error { get; set; }
    }
}
