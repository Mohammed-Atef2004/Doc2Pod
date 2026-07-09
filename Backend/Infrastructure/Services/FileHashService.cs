using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using UglyToad.PdfPig;


namespace Infrastructure.Services
{
    public class FileHashService : IFileHashService
    {
        public async Task<string> GenerateHashAsync(IFormFile file)
        {
            using var sha256 = SHA256.Create();
            using var stream = file.OpenReadStream();
            var hashBytes =
                await sha256.ComputeHashAsync(stream);

            return Convert.ToHexString(hashBytes);
        }

        public async Task<int> GetPageCountAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var pdf = PdfDocument.Open(stream);
            return pdf.NumberOfPages;
        }
    }
}
