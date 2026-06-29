using Domain.Documents;


namespace Domain.Interfaces.Repositories
{
    public interface IDocumentRepository : IGenericRepository<Document>
    {
        public IQueryable<Document> GetQueryableDocumentsByUserId(Guid userId);
        public Task<bool> IsHashExistsAsync(
           Guid userId, string fileHash);
    }
}
