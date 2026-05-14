using System.Net.Http.Json;
using StudentOnboardingApp.Helpers;
using StudentOnboardingApp.Models.Auth;
using StudentOnboardingApp.Models.Common;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly HttpClient _publicClient;
    private readonly HttpClient _authenticatedClient;
    private readonly ITokenStorageService _tokenStorage;

    public AuthService(
        IHttpClientFactory httpClientFactory,
        ITokenStorageService tokenStorage)
    {
        _publicClient = httpClientFactory.CreateClient(Constants.PublicApiClient);
        _authenticatedClient = httpClientFactory.CreateClient(Constants.AuthenticatedApiClient);
        _tokenStorage = tokenStorage;
    }

    public async Task<ApiResponse<string>> SignupAsync(SignupRequest request)
    {
        var response = await _publicClient.PostAsJsonAsync("auth/signup", request);
        return await HttpResponseParser.ParseAsync<string>(response);
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var response = await _publicClient.PostAsJsonAsync("auth/login", request);
        var result = await HttpResponseParser.ParseAsync<AuthResponse>(response);

        if (result.Success && result.Data != null)
        {
            await _tokenStorage.SaveTokensAsync(
                result.Data.AccessToken,
                result.Data.RefreshToken,
                result.Data.ExpiresAt);
            await _tokenStorage.SaveUserAsync(result.Data.User);
        }

        return result;
    }

    public async Task<ApiResponse<string>> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var response = await _publicClient.PostAsJsonAsync("auth/verify-otp", request);
        return await HttpResponseParser.ParseAsync<string>(response);
    }

    public async Task<ApiResponse<string>> ResendOtpAsync(ResendOtpRequest request)
    {
        var response = await _publicClient.PostAsJsonAsync("auth/resend-otp", request);
        return await HttpResponseParser.ParseAsync<string>(response);
    }

    public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var response = await _publicClient.PostAsJsonAsync("auth/forgot-password", request);
        return await HttpResponseParser.ParseAsync<string>(response);
    }

    public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var response = await _publicClient.PostAsJsonAsync("auth/reset-password", request);
        return await HttpResponseParser.ParseAsync<string>(response);
    }

    public async Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var response = await _authenticatedClient.PostAsJsonAsync("auth/change-password", request);
        return await HttpResponseParser.ParseAsync<string>(response);
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var response = await _publicClient.PostAsJsonAsync("auth/refresh-token", request);
        var result = await HttpResponseParser.ParseAsync<AuthResponse>(response);

        if (result.Success && result.Data != null)
        {
            await _tokenStorage.SaveTokensAsync(
                result.Data.AccessToken,
                result.Data.RefreshToken,
                result.Data.ExpiresAt);
        }

        return result;
    }

    public async Task<ApiResponse<string>> LogoutAsync()
    {
        var refreshToken = await _tokenStorage.GetRefreshTokenAsync();
        var response = await _authenticatedClient.PostAsJsonAsync("auth/logout",
            new RefreshTokenRequest { RefreshToken = refreshToken ?? string.Empty });
        var result = await HttpResponseParser.ParseAsync<string>(response);
        await _tokenStorage.ClearAllAsync();
        return result;
    }
}
