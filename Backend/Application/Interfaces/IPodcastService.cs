using Application.Features.Podcasts.Commands.GeneratePodcast;


namespace Application.Interfaces
{
    public interface IPodcastService
    {
        public Task ProcessPodcastGenerationAsync(Guid podcastId, string filePath, GeneratePodcastCommand command);

    }
}
