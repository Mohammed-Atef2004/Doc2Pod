using Domain.Users;

namespace Domain.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IDocumentRepository Document { get; }
        IPodcastRepository Podcast { get; }
        Task<int> CompleteAsync(CancellationToken cancellationToken = default);
        Task<int> RollbackAsync(CancellationToken cancellationToken = default);
    }
}