using Application.Features.Podcasts.DTOs.Requests;
using Application.Features.Podcasts.DTOs.Responses;
using Application.Interfaces;
using AutoMapper;
using Infrastructure.Services.PythonService.DTOs;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
namespace Infrastructure.Services.PythonService
{
    public class PythonRagService : IPythonRagService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        public PythonRagService(HttpClient httpClient, IConfiguration config, IMapper mapper)
        {
            _httpClient = httpClient;
            _config = config;
            _mapper = mapper;
        }

        public async Task<string> StartGenerationAsync(GeneratePodcastRequest request)
        {
            var pythonRequest = _mapper.Map<GenerateRequest>(request);

            var response = await _httpClient.PostAsJsonAsync("generate", pythonRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    throw new Exception("The AI Server is currently busy generating another podcast. Please wait.");

                throw new Exception($"Python API Error ({response.StatusCode}): {errorContent}");
            }

            var data = await response.Content.ReadFromJsonAsync<GenerateStartResponse>();

            return data?.TaskId ?? throw new Exception("No TaskId returned");
        }

        public async Task<PodcastGenerationStatusResponse> GetStatusAsync(string taskId)
        {
            var response = await _httpClient.GetAsync($"status/{taskId}");

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                throw new HttpRequestException($"Failed to get status: {response.StatusCode} - {content}");
            }

            var data = await response.Content.ReadFromJsonAsync<TaskStatusResponse>();

            if (data == null)
                throw new Exception("Invalid response from Python");

            return _mapper.Map<PodcastGenerationStatusResponse>(data);
        }
    }
}