using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using StudentOnboardingApp.Models.Common;
using StudentOnboardingApp.Models.Profile;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.Services.Implementations;

public class ProfileService : IProfileService
{
    private readonly HttpClient _client;

    public ProfileService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient(Constants.AuthenticatedApiClient);
    }

    public async Task<ApiResponse<StudentProfileDto>> GetProfileAsync()
    {
        try
        {
            var response = await _client.GetAsync("student/profile");
            var json = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json))
                return new ApiResponse<StudentProfileDto>
                {
                    Success = false,
                    Message = $"Server returned {(int)response.StatusCode} with no body."
                };

            var result = JsonSerializer.Deserialize<ApiResponse<StudentProfileDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result ?? new ApiResponse<StudentProfileDto> { Success = false, Message = $"Parse failed. Raw: {json[..Math.Min(json.Length, 200)]}" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<StudentProfileDto> { Success = false, Message = "Something went wrong. Please try again." };
        }
    }

    public async Task<ApiResponse<StudentProfileDto>> UpdateProfileAsync(UpdateProfileRequest request)
    {
        try
        {
            var response = await _client.PutAsJsonAsync("student/profile", request);
            var json = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json))
                return new ApiResponse<StudentProfileDto>
                {
                    Success = false,
                    Message = $"Server returned {(int)response.StatusCode} with no body."
                };

            var result = JsonSerializer.Deserialize<ApiResponse<StudentProfileDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result != null)
                return result;

            if (response.IsSuccessStatusCode)
                return new ApiResponse<StudentProfileDto> { Success = true, Message = "Profile updated." };

            return new ApiResponse<StudentProfileDto> { Success = false, Message = $"Server: {json[..Math.Min(json.Length, 200)]}" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<StudentProfileDto> { Success = false, Message = "Something went wrong. Please try again." };
        }
    }

    public async Task<ApiResponse<StudentProfileDto>> UploadPhotoAsync(Stream photoStream, string fileName)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(photoStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(streamContent, "photo", fileName);

            var response = await _client.PostAsync("student/profile/photo", content);
            var json = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json))
                return response.IsSuccessStatusCode
                    ? new ApiResponse<StudentProfileDto> { Success = true, Message = "Photo uploaded." }
                    : new ApiResponse<StudentProfileDto> { Success = false, Message = "Upload failed." };

            var result = JsonSerializer.Deserialize<ApiResponse<StudentProfileDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result != null)
                return result;

            return response.IsSuccessStatusCode
                ? new ApiResponse<StudentProfileDto> { Success = true, Message = "Photo uploaded." }
                : new ApiResponse<StudentProfileDto> { Success = false, Message = "Upload failed." };
        }
        catch (Exception ex)
        {
            return new ApiResponse<StudentProfileDto> { Success = false, Message = "Something went wrong. Please try again." };
        }
    }

    public async Task<ApiResponse<string>> UploadDocumentAsync(Stream docStream, string fileName, string documentType)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(docStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(streamContent, "document", fileName);
            content.Add(new StringContent(documentType), "documentType");

            var response = await _client.PostAsync("student/profile/document", content);
            var json = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json))
                return new ApiResponse<string> { Success = false, Message = "Empty response from server." };

            var result = JsonSerializer.Deserialize<ApiResponse<string>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result ?? new ApiResponse<string> { Success = false, Message = "Failed to parse response." };
        }
        catch (Exception ex)
        {
            return new ApiResponse<string> { Success = false, Message = "Something went wrong. Please try again." };
        }
    }
}
