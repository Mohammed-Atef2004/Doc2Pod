using System.Text.Json.Serialization;

namespace Infrastructure.Services.PythonService.DTOs
{
    public class GenerateStartResponse
    {
        [JsonPropertyName("task_id")]
        public string TaskId { get; set; }
    }
}
