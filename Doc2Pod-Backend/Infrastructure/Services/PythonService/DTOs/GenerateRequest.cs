using System.Text.Json.Serialization;

namespace Infrastructure.Services.PythonService.DTOs
{
    public class GenerateRequest
    {
        [JsonPropertyName("file_key")]
        public string FileKey { get; set; }

        [JsonPropertyName("mode")]
        public int Mode { get; set; }

        [JsonPropertyName("topic")]
        public string? Topic { get; set; }

        [JsonPropertyName("start_page")]
        public int? StartPage { get; set; }

        [JsonPropertyName("end_page")]
        public int? EndPage { get; set; }
    }
}
