using Domain.Podcasts;
using System.Linq.Expressions;

namespace Domain.Interfaces.Repositories
{
    public interface IPodcastRepository : IGenericRepository<Podcast>
    {
        public Task<bool> ExistsAsync(Expression<Func<Podcast, bool>> predicate);

        public Task<IEnumerable<Podcast>> GetCompletedPodcastsByUserIdAsync(Guid userId);

        public Task<Podcast?> GetRunningPodcastByUserIdAsync(Guid userId);

        public Task<Podcast?> GetByIdWithDocumentAsync(Guid id);

        public IQueryable<Podcast> GetQueryablePodcastByUserId(Guid userId);
    }
}
