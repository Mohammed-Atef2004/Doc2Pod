using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IFileStorageService
    {
        public Task<string> SaveFileAsync(IFormFile file);
        public Task<string> GetSignedUrlAsync(string bucketName, string filePath, int expiresIn = 60);
    }
}
