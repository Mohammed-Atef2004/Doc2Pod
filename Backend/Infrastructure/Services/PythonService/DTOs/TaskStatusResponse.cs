using System.Text.Json.Serialization;

namespace Infrastructure.Services.PythonService.DTOs
{
    public class TaskStatusResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("script_path")]
        public string ScriptPath { get; set; }

        [JsonPropertyName("audio_path")]
        public string AudioPath { get; set; }

        [JsonPropertyName("error")]
        public string ?Error { get; set; }
    }
}
