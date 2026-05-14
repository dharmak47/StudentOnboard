using Student_Onboarding_Platform.Models.DTOs.Auth;
using Student_Onboarding_Platform.Models.DTOs.Common;

namespace Student_Onboarding_Platform.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<string>> SignupAsync(SignupRequest request);
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent);
    Task<ApiResponse<string>> VerifyOtpAsync(VerifyOtpRequest request);
    Task<ApiResponse<string>> ResendOtpAsync(ResendOtpRequest request);
    Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<ApiResponse<string>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<ApiResponse<string>> LogoutAsync(Guid userId, string refreshToken);
    Task<ApiResponse<CheckApprovalStatusResponse>> CheckApprovalStatusAsync(CheckApprovalStatusRequest request);
}
