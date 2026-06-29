using Application.Features.Podcasts.Query.GetPodcastDetails;
using Domain.Podcasts;

namespace Application.Features.Podcasts.Mapping
{
    public partial class PodcastProfile
    {
        public void GetPodcastDetailsMapping()
        {
            CreateMap<Podcast, GetPodcastDetailsResponse>()
                .ForCtorParam("Mode", opt => opt.MapFrom(src => src.Mode))
                .ForCtorParam("PodcastName", opt => opt.MapFrom(src => src.Document.FileName))
                .ForCtorParam("CreatedAt", opt => opt.MapFrom(src => src.CreatedAt))
                .ForCtorParam("Topic", opt => opt.MapFrom(src => src.Topic))
                .ForCtorParam("StartPage", opt => opt.MapFrom(src => src.StartPage))
                .ForCtorParam("EndPage", opt => opt.MapFrom(src => src.EndPage));
        }
    }

}
