using Application.Features.Podcasts.DTOs.Requests;
using Application.Features.Podcasts.DTOs.Responses;

namespace Application.Interfaces
{
    public interface IPythonRagService
    {

        Task<string> StartGenerationAsync(GeneratePodcastRequest request);
        Task<PodcastGenerationStatusResponse> GetStatusAsync(string taskId);
    }
}
