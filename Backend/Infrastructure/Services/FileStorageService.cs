using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _key;

        public FileStorageService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _baseUrl = config["Supabase:Url"];
            _key = config["Supabase:Key"];
        }

        private void SetHeaders()
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _key);

            if (!_httpClient.DefaultRequestHeaders.Contains("apikey"))
            {
                _httpClient.DefaultRequestHeaders.Add("apikey", _key);
            }
        }

        private async Task<string> HandleResponse(HttpResponseMessage response, string errorPrefix)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"{errorPrefix}: {content}");

            return content;
        }

        public async Task<string> GetSignedUrlAsync(string bucketName, string filePath, int expiresIn = 500)
        {
            SetHeaders();

            var url = $"{_baseUrl}/storage/v1/object/sign/{bucketName}/{filePath}";

            var body = new
            {
                expiresIn = expiresIn
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(url, content);

            var result = await HandleResponse(response, "Failed to generate signed URL");

            var json = JsonSerializer.Deserialize<JsonElement>(result);

            var signedPath = json.GetProperty("signedURL").GetString();

            return $"{_baseUrl}/storage/v1{signedPath}";
        }


        public async Task<string> SaveFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            SetHeaders();

            var bucket = "PDFs";

            var extension = Path.GetExtension(file.FileName);
            var safeFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = safeFileName;

            var requestUrl = $"{_baseUrl}/storage/v1/object/{bucket}/{filePath}";

            using var stream = file.OpenReadStream();
            var content = new StreamContent(stream);

            content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Headers.Add("x-upsert", "true");

            var response = await _httpClient.PostAsync(requestUrl, content);
            await HandleResponse(response, "Supabase Storage Error (SaveFile)");

            return filePath;
        }

    }
}