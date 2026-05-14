using System.Text.Json;
using StudentOnboardingApp.Models.Common;

namespace StudentOnboardingApp.Helpers;

/// <summary>
/// Safely parses HTTP responses into ApiResponse, handling empty bodies,
/// non-JSON responses, and ValidationProblemDetails from ASP.NET.
/// </summary>
public static class HttpResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<ApiResponse<T>> ParseAsync<T>(HttpResponseMessage response)
    {
        string body;
        try
        {
            body = await response.Content.ReadAsStringAsync();
        }
        catch
        {
            return Fail<T>("Unable to read server response. Please try again.");
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return Fail<T>(response.IsSuccessStatusCode
                ? "Server returned an empty response."
                : $"Request failed (HTTP {(int)response.StatusCode}). Please try again.");
        }

        // Try to deserialize as ApiResponse<T>
        try
        {
            var result = JsonSerializer.Deserialize<ApiResponse<T>>(body, JsonOptions);
            if (result != null && !string.IsNullOrEmpty(result.Message))
                return result;

            // Response was JSON but not in ApiResponse format (e.g. ValidationProblemDetails)
            if (result is { Success: false } && string.IsNullOrEmpty(result.Message))
                return Fail<T>(ExtractErrorFromBody(body, response));

            return result ?? Fail<T>("Failed to parse server response.");
        }
        catch (JsonException)
        {
            // Body is not valid JSON or doesn't map to ApiResponse<T>
            return Fail<T>(ExtractErrorFromBody(body, response));
        }
    }

    private static string ExtractErrorFromBody(string body, HttpResponseMessage response)
    {
        // Try to extract errors from ASP.NET ValidationProblemDetails format
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // Check for "errors" object (ValidationProblemDetails)
            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                var messages = new List<string>();
                foreach (var prop in errors.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in prop.Value.EnumerateArray())
                            messages.Add(item.GetString() ?? "");
                    }
                }
                if (messages.Count > 0)
                    return string.Join(" ", messages);
            }

            // Check for "message" or "title" property
            if (root.TryGetProperty("message", out var msg))
                return msg.GetString() ?? "Request failed.";
            if (root.TryGetProperty("title", out var title))
                return title.GetString() ?? "Request failed.";
        }
        catch { /* not valid JSON at all */ }

        return $"Request failed (HTTP {(int)response.StatusCode}). Please try again.";
    }

    private static ApiResponse<T> Fail<T>(string message) =>
        new() { Success = false, Message = message };
}
