using Domain.Documents;
using Domain.Interfaces.Repositories;
using Infrastructure.Presistence.Data;
using Infrastructure.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class DocumentRepository :
        GenericRepository<Document>, IDocumentRepository
    {
        private readonly AppDbContext _context;
        public DocumentRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Document> GetQueryableDocumentsByUserId(Guid userId)
        {
            return _context.Documents.Where(p => p.UserId == userId && p.IsDeleted == false).AsQueryable();
        }

        public async Task<bool> IsHashExistsAsync(
           Guid userId, string fileHash)
        {
            return await _context.Documents.AnyAsync(
                x =>
                    x.UserId == userId &&
                    x.FileHash == fileHash
            );
        }
    }

}
