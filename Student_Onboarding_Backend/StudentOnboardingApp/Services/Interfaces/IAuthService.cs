using StudentOnboardingApp.Models.Auth;
using StudentOnboardingApp.Models.Common;

namespace StudentOnboardingApp.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<string>> SignupAsync(SignupRequest request);
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<string>> VerifyOtpAsync(VerifyOtpRequest request);
    Task<ApiResponse<string>> ResendOtpAsync(ResendOtpRequest request);
    Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordRequest request);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<ApiResponse<string>> LogoutAsync();
}
