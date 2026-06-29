using Domain.SharedKernel;

namespace Domain.Podcasts.Errors
{
    public static class GeneratePodcastErrors
    {
        public static readonly Error FullPodcastAlreadyExists =
        new("Podcast.FullGenerationAlreadyExists", "A full podcast has already been generated for this document.");

        public static readonly Error DocumentNotFound =
            new("Document.NotFound", "The document with the provided ID was not found.");

        public static readonly Error PodcastAlreadyRunning =
            new("Podcast.AlreadyRunning",
           "You already have a podcast being generated. Please wait for it to finish.");

        public static readonly Error InvalidStartPage =
    new(
        "Podcast.InvalidStartPage",
        "Start page must be greater than 0."
    );

        public static readonly Error InvalidEndPage =
            new(
                "Podcast.InvalidEndPage",
                "End page must be greater than 0."
            );

        public static readonly Error InvalidPageRange =
            new(
                "Podcast.InvalidPageRange",
                "Start page cannot be greater than end page."
            );
        public static Error InvalidEndPageExceedsTotal(int totalPages) =>
            new(
                "Podcast.InvalidEndPageExceedsTotal",
                $"End page exceeds the total number of pages in the document ({totalPages} pages)."
            );

    }
}
