using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Student_Onboarding_Platform.Models.Settings;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Services.Implementations;

public class BytescaleStorageService : IFileStorageService
{
    private readonly HttpClient _httpClient;
    private readonly BytescaleSettings _settings;
    private readonly ILogger<BytescaleStorageService> _logger;

    public BytescaleStorageService(
        IHttpClientFactory httpClientFactory,
        IOptions<BytescaleSettings> settings,
        ILogger<BytescaleStorageService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Bytescale");
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        var url = $"https://api.bytescale.com/v2/accounts/{_settings.AccountId}/uploads/binary?fileName={Uri.EscapeDataString(fileName)}";

        using var content = new StreamContent(fileStream);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        request.Content = content;

        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("Bytescale response: {StatusCode} {Response}", response.StatusCode, responseBody);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Bytescale upload failed: {StatusCode} {Response}", response.StatusCode, responseBody);
            throw new Exception($"File upload failed: {response.StatusCode}");
        }

        var json = JsonDocument.Parse(responseBody);

        // Bytescale response may use "fileUrl" or nested "file.url" depending on API version
        string? fileUrl = null;
        if (json.RootElement.TryGetProperty("fileUrl", out var fileUrlProp))
        {
            fileUrl = fileUrlProp.GetString();
        }
        else if (json.RootElement.TryGetProperty("fileUrl", out var altProp))
        {
            fileUrl = altProp.GetString();
        }

        // Fallback: construct URL from accountId and filePath
        if (string.IsNullOrEmpty(fileUrl) && json.RootElement.TryGetProperty("filePath", out var filePathProp))
        {
            var filePath = filePathProp.GetString();
            fileUrl = $"https://upcdn.io/{_settings.AccountId}/raw{filePath}";
        }

        if (string.IsNullOrEmpty(fileUrl))
        {
            _logger.LogError("Could not extract file URL from Bytescale response: {Response}", responseBody);
            throw new Exception("Bytescale response missing file URL");
        }

        _logger.LogInformation("File uploaded to Bytescale: {FileName} -> {FileUrl}", fileName, fileUrl);
        return fileUrl;
    }
}
