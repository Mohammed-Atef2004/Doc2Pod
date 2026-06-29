using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IFileHashService
    {
        public Task<string> GenerateHashAsync(IFormFile file);
        public Task<int> GetPageCountAsync(IFormFile file);
    }
}
