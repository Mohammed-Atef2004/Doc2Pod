using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Podcasts;
using Infrastructure.Presistence.Data;
using Infrastructure.Repositories.Shared;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class PodcastRepository : GenericRepository<Podcast>, IPodcastRepository
    {
        private readonly AppDbContext _context;
        public PodcastRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<bool> ExistsAsync(Expression<Func<Podcast, bool>> predicate)
        {
            return await _context.Podcasts.AnyAsync(predicate);
        }

        public async Task<Podcast?> GetByIdWithDocumentAsync(Guid id)
        {
            return await _context.Podcasts
                .Include(x => x.Document)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<Podcast>> GetCompletedPodcastsByUserIdAsync(Guid userId)
        {
            return await _context.Podcasts
                .AsNoTracking()
                .Include(p => p.Document)
                .Where(p => p.UserId == userId && p.Status == PodcastStatus.Completed)
                .ToListAsync();
        }

        public IQueryable<Podcast> GetQueryablePodcastByUserId(Guid userId)
        {
            return _context.Podcasts
                .AsNoTracking()
                .Where(p => p.UserId == userId && p.Status == PodcastStatus.Completed && p.IsDeleted == false).AsQueryable();
        }

        public async Task<Podcast?> GetRunningPodcastByUserIdAsync(Guid userId)
        {
            return await _context.Podcasts
                .Include(p => p.Document)
                .Where(x =>
                    x.UserId == userId &&
                   (x.Status == PodcastStatus.Processing ||
                    x.Status == PodcastStatus.Pending))
                .FirstOrDefaultAsync();
        }
    }

}
