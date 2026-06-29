using Domain.Interfaces.Repositories;
using Domain.SharedKernel;
using Infrastructure.Presistence.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Shared
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IMediator _mediator;
        public IDocumentRepository Document { get; private set; }
        public IPodcastRepository Podcast { get; private set; }

        public UnitOfWork(AppDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
            Document = new DocumentRepository(_context);
            Podcast = new PodcastRepository(_context);

        }


        public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
        {

            var aggregateRoots = _context.ChangeTracker
                .Entries<IAggregateRoot>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            var domainEvents = aggregateRoots
                .SelectMany(e => e.DomainEvents)
                .ToList();

            foreach (var entity in aggregateRoots)
            {
                entity.ClearDomainEvents();
            }

            var result = await _context.SaveChangesAsync(cancellationToken);

            foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }

            return result;
        }


        public void Dispose()
        {
            _context?.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<int> RollbackAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }
            await Task.CompletedTask;
            return 0;
        }
    }
}
