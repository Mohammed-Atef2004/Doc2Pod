using Domain.SharedKernel;

namespace Domain.Podcasts.Errors
{

    public static class GetPodcastErrors
    {
        public static readonly Error NotFound =
        new("Podcast.NotFound", "The requested podcast was not found.");

        public static readonly Error NotReady =
            new("Podcast.NotReady", "The podcast is still being processed and is not ready for playback.");

        public static readonly Error MissingAudioPath =
            new("Podcast.MissingAudioPath", "The podcast generation is complete, but the audio file path is missing.");

        public static readonly Error StorageAccessFailed =
            new("Podcast.StorageAccessFailed", "Could not retrieve the audio file from the storage service.");
        public static readonly Error Unauthorized =
            new("Podcast.Unauthorized",
        "You are not authorized to access this podcast.");
    }
}


