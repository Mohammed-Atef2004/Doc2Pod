using Application.Features.Podcasts.DTOs.Requests;
using Application.Features.Podcasts.DTOs.Responses;
using AutoMapper;
using Infrastructure.Services.PythonService.DTOs;

namespace Infrastructure.Services.PythonService.Mapping
{
    public class PythonMappingProfile : Profile
    {
        public PythonMappingProfile()
        {
            CreateMap<TaskStatusResponse, PodcastGenerationStatusResponse>();
            CreateMap<GeneratePodcastRequest, GenerateRequest>();
        }
    }
}

