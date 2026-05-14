using StudentOnboardingApp.Models.Auth;

namespace StudentOnboardingApp.Services.Interfaces;

public interface ITokenStorageService
{
    Task SaveTokensAsync(string accessToken, string refreshToken, DateTime expiresAt);
    Task<string?> GetAccessTokenAsync();
    Task<string?> GetRefreshTokenAsync();
    Task<DateTime?> GetTokenExpiryAsync();
    Task SaveUserAsync(UserDto user);
    Task<UserDto?> GetUserAsync();
    Task ClearAllAsync();
    Task<bool> IsAuthenticatedAsync();
}
