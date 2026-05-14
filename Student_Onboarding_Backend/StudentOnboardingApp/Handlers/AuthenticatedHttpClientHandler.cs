using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommunityToolkit.Mvvm.Messaging;
using StudentOnboardingApp.Models.Auth;
using StudentOnboardingApp.Models.Common;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.Handlers;

public class LogoutMessage { }

public class AuthenticatedHttpClientHandler : DelegatingHandler
{
    private readonly ITokenStorageService _tokenStorage;
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly SemaphoreSlim _refreshSemaphore = new(1, 1);

    public AuthenticatedHttpClientHandler(
        ITokenStorageService tokenStorage,
        IHttpClientFactory httpClientFactory)
    {
        _tokenStorage = tokenStorage;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Proactively refresh if token expires within 2 minutes
        var expiry = await _tokenStorage.GetTokenExpiryAsync();
        if (expiry.HasValue && expiry.Value <= DateTime.UtcNow.AddMinutes(2))
        {
            await TryProactiveRefreshAsync(cancellationToken);
        }

        var token = await _tokenStorage.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            response = await HandleUnauthorizedAsync(request, cancellationToken);
        }

        return response;
    }

    private async Task TryProactiveRefreshAsync(CancellationToken cancellationToken)
    {
        if (!await _refreshSemaphore.WaitAsync(0, cancellationToken))
            return; // Another thread is already refreshing

        try
        {
            // Re-check expiry inside the lock
            var expiry = await _tokenStorage.GetTokenExpiryAsync();
            if (expiry.HasValue && expiry.Value > DateTime.UtcNow.AddMinutes(2))
                return; // Already refreshed by another thread

            var refreshToken = await _tokenStorage.GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken))
                return;

            var publicClient = _httpClientFactory.CreateClient(Constants.PublicApiClient);
            var refreshResponse = await publicClient.PostAsJsonAsync(
                "auth/refresh-token",
                new RefreshTokenRequest { RefreshToken = refreshToken },
                cancellationToken);

            if (refreshResponse.IsSuccessStatusCode)
            {
                var result = await refreshResponse.Content
                    .ReadFromJsonAsync<ApiResponse<AuthResponse>>(cancellationToken: cancellationToken);

                if (result?.Success == true && result.Data != null)
                {
                    await _tokenStorage.SaveTokensAsync(
                        result.Data.AccessToken,
                        result.Data.RefreshToken,
                        result.Data.ExpiresAt);
                }
            }
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    private async Task<HttpResponseMessage> HandleUnauthorizedAsync(
        HttpRequestMessage originalRequest, CancellationToken cancellationToken)
    {
        await _refreshSemaphore.WaitAsync(cancellationToken);
        try
        {
            // Double-check: maybe another thread already refreshed
            var currentToken = await _tokenStorage.GetAccessTokenAsync();
            var expiry = await _tokenStorage.GetTokenExpiryAsync();
            if (expiry.HasValue && expiry.Value > DateTime.UtcNow)
            {
                // Token was already refreshed by another request
                var retryRequest = await CloneRequestAsync(originalRequest);
                retryRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", currentToken);
                return await base.SendAsync(retryRequest, cancellationToken);
            }

            var refreshToken = await _tokenStorage.GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken))
            {
                await ForceLogoutAsync();
                return CreateUnauthorizedResponse("Session expired. Please log in again.");
            }

            // Use the public API client to avoid recursion
            var publicClient = _httpClientFactory.CreateClient(Constants.PublicApiClient);
            var refreshResponse = await publicClient.PostAsJsonAsync(
                "auth/refresh-token",
                new RefreshTokenRequest { RefreshToken = refreshToken },
                cancellationToken);

            if (refreshResponse.IsSuccessStatusCode)
            {
                var result = await refreshResponse.Content
                    .ReadFromJsonAsync<ApiResponse<AuthResponse>>(cancellationToken: cancellationToken);

                if (result?.Success == true && result.Data != null)
                {
                    await _tokenStorage.SaveTokensAsync(
                        result.Data.AccessToken,
                        result.Data.RefreshToken,
                        result.Data.ExpiresAt);

                    var retryRequest = await CloneRequestAsync(originalRequest);
                    retryRequest.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", result.Data.AccessToken);
                    return await base.SendAsync(retryRequest, cancellationToken);
                }
            }

            await ForceLogoutAsync();
            return CreateUnauthorizedResponse("Session expired. Please log in again.");
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    private async Task ForceLogoutAsync()
    {
        await _tokenStorage.ClearAllAsync();
        MainThread.BeginInvokeOnMainThread(() =>
        {
            WeakReferenceMessenger.Default.Send(new LogoutMessage());
        });
    }

    private static HttpResponseMessage CreateUnauthorizedResponse(string message)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new { success = false, message });
        return new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);
        if (request.Content != null)
        {
            var content = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(content);
            if (request.Content.Headers.ContentType != null)
                clone.Content.Headers.ContentType = request.Content.Headers.ContentType;
        }
        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        return clone;
    }
}
