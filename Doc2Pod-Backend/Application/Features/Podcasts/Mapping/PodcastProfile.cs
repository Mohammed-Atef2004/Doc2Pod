using AutoMapper;


namespace Application.Features.Podcasts.Mapping
{
    public partial class PodcastProfile : Profile
    {
        public PodcastProfile()
        {
            GetUserPodcastsMapping();

            GetPodcastDetailsMapping();
        }
    }
}
