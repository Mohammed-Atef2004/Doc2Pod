using Domain.Enums;
using Domain.Podcasts;
using Domain.SharedKernel;
using Domain.Users;

namespace Domain.Documents
{
    public class Document : Entity<Guid>, ISoftDeletable
    {
        public string FileName { get; private set; }
        public Guid UserId { get; private set; }
        public string FilePath { get; private set; }
        public DateTime UploadedAt { get; private set; }
        public string FileHash { get; private set; }
        public int PageCount { get; private set; }

        private readonly List<Podcast> _podcasts = new List<Podcast>();
        public IReadOnlyCollection<Podcast> Podcasts => _podcasts.AsReadOnly();

        public User User { get; private set; }

        protected Document() { }

        public Document(Guid userId, string fileName, string filePath, string fileHash, int pageCount)
        {
            UserId = userId;
            FileName = fileName;
            FilePath = filePath;
            FileHash = fileHash;
            UploadedAt = DateTime.UtcNow;
            PageCount = pageCount;
        }

        public Podcast AddPodcast(Guid userId, PodcastMode mode, string? topic, int? startPage, int? endPage, PodcastStatus podcastStatus)
        {

            var podcast = new Podcast(
                userId,
                Id,
                mode,
                topic,
                startPage,
                endPage,
                podcastStatus
            );

            _podcasts.Add(podcast);

            return podcast;
        }
    }
}