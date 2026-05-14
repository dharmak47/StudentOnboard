using System.Text.Json;
using StudentOnboardingApp.Models.Auth;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.Services.Implementations;

public class TokenStorageService : ITokenStorageService
{
    public async Task SaveTokensAsync(string accessToken, string refreshToken, DateTime expiresAt)
    {
        await SecureStorage.Default.SetAsync(Constants.AccessTokenKey, accessToken);
        await SecureStorage.Default.SetAsync(Constants.RefreshTokenKey, refreshToken);
        await SecureStorage.Default.SetAsync(Constants.TokenExpiryKey, expiresAt.ToString("O"));
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(Constants.AccessTokenKey);
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(Constants.RefreshTokenKey);
    }

    public async Task<DateTime?> GetTokenExpiryAsync()
    {
        var expiryStr = await SecureStorage.Default.GetAsync(Constants.TokenExpiryKey);
        if (DateTime.TryParse(expiryStr, out var expiry))
            return expiry;
        return null;
    }

    public async Task SaveUserAsync(UserDto user)
    {
        var json = JsonSerializer.Serialize(user);
        await SecureStorage.Default.SetAsync(Constants.UserJsonKey, json);
    }

    public async Task<UserDto?> GetUserAsync()
    {
        var json = await SecureStorage.Default.GetAsync(Constants.UserJsonKey);
        if (string.IsNullOrEmpty(json))
            return null;
        return JsonSerializer.Deserialize<UserDto>(json);
    }

    public Task ClearAllAsync()
    {
        SecureStorage.Default.RemoveAll();
        return Task.CompletedTask;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetAccessTokenAsync();
        if (string.IsNullOrEmpty(token))
            return false;

        var expiry = await GetTokenExpiryAsync();
        if (expiry.HasValue && expiry.Value > DateTime.UtcNow)
            return true;

        // Access token expired, check if refresh token exists
        var refreshToken = await GetRefreshTokenAsync();
        return !string.IsNullOrEmpty(refreshToken);
    }
}
