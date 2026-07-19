using Microsoft.Extensions.Options;
using Student_Onboarding_Platform.Models.DTOs.Auth;
using Student_Onboarding_Platform.Models.DTOs.Common;
using Student_Onboarding_Platform.Models.Entities;
using Student_Onboarding_Platform.Models.Enums;
using Student_Onboarding_Platform.Models.Settings;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly ISessionService _sessionService;
    private readonly ILoginAttemptService _loginAttemptService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly INotificationService _notificationService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    private const int MaxFailedAttemptsPerWindow = 5;
    private static readonly TimeSpan FailedAttemptWindow = TimeSpan.FromMinutes(15);

    public AuthService(
        IUserService userService,
        ITokenService tokenService,
        IOtpService otpService,
        IEmailService emailService,
        ISessionService sessionService,
        ILoginAttemptService loginAttemptService,
        IPasswordHasher passwordHasher,
        INotificationService notificationService,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _userService = userService;
        _tokenService = tokenService;
        _otpService = otpService;
        _emailService = emailService;
        _sessionService = sessionService;
        _loginAttemptService = loginAttemptService;
        _passwordHasher = passwordHasher;
        _notificationService = notificationService;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<ApiResponse<AuthResponse>> SignupAsync(SignupRequest request)
    {
        _logger.LogInformation("Signup attempt for {Email}", request.Email);

        var existingUser = await _userService.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Signup failed: email {Email} already registered", request.Email);
            return ApiResponse<AuthResponse>.Fail("An account with this email already exists.");
        }

        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            var existingPhone = await _userService.GetByPhoneNumberAsync(request.PhoneNumber);
            if (existingPhone != null)
            {
                _logger.LogWarning("Signup failed: phone {Phone} already registered", request.PhoneNumber);
                return ApiResponse<AuthResponse>.Fail("An account with this phone number already exists.");
            }
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email.ToLowerInvariant(),
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordHasher.Hash(request.Password),
            EmailVerified = true,
            PhoneVerified = false,
            IsActive = true,
            IsDeleted = false,
            Role = nameof(UserRole.Student)
        };

        await _userService.CreateAsync(user);
        _logger.LogInformation("User created: {UserId} ({Email})", user.Id, user.Email);

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        await _sessionService.CreateSessionAsync(
            user.Id, refreshToken, "Web", "Browser",
            null, null, expiresAt);

        _logger.LogInformation("Session created for user {UserId}", user.Id);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            User = MapToUserDto(user)
        }, "Signup successful. You can now access the platform.");
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent)
    {
        _logger.LogInformation("Login attempt for {Email}", request.Email);
        var email = request.Email.ToLowerInvariant();

        // Check rate limiting
        var recentFailures = await _loginAttemptService.GetRecentFailedAttemptsAsync(email, FailedAttemptWindow);
        if (recentFailures >= MaxFailedAttemptsPerWindow)
        {
            _logger.LogWarning("Login blocked for {Email}: too many failed attempts", email);
            await _loginAttemptService.LogAttemptAsync(email, ipAddress, userAgent, false, nameof(FailureReason.AccountLocked));
            return ApiResponse<AuthResponse>.Fail("Too many failed login attempts. Please try again later.");
        }

        var user = await _userService.GetByEmailAsync(email);
        if (user == null)
        {
            await _loginAttemptService.LogAttemptAsync(email, ipAddress, userAgent, false, nameof(FailureReason.UserNotFound));
            return ApiResponse<AuthResponse>.Fail("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            await _loginAttemptService.LogAttemptAsync(email, ipAddress, userAgent, false, nameof(FailureReason.AccountInactive));
            return ApiResponse<AuthResponse>.Fail("Your account is inactive. Please contact support.");
        }

        if (user.IsDeleted)
        {
            await _loginAttemptService.LogAttemptAsync(email, ipAddress, userAgent, false, nameof(FailureReason.AccountDeleted));
            return ApiResponse<AuthResponse>.Fail("Invalid email or password.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            await _loginAttemptService.LogAttemptAsync(email, ipAddress, userAgent, false, nameof(FailureReason.InvalidPassword));
            return ApiResponse<AuthResponse>.Fail("Invalid email or password.");
        }

        if (!user.EmailVerified)
        {
            await _loginAttemptService.LogAttemptAsync(email, ipAddress, userAgent, false, nameof(FailureReason.EmailNotVerified));

            // Generate and send a new OTP for verification
            var otpCode = await _otpService.GenerateAndStoreOtpAsync(user.Id, user.Email, nameof(OtpType.EmailVerification));
            // await _emailService.SendOtpEmailAsync(user.Email, otpCode, "Email Verification");

            return ApiResponse<AuthResponse>.Fail("Email not verified. A new verification OTP has been sent to your email.");
        }

        //// Check approval status (Phase 2)
        //if (user.ApprovalStatus == nameof(ApprovalStatus.Pending))
        //{
        //    await _loginAttemptService.LogAttemptAsync(email, ipAddress, userAgent, false, nameof(FailureReason.PendingApproval));
        //    return ApiResponse<AuthResponse>.Fail("Your account is pending admin approval. You will be notified once approved.");
        //}

        if (user.ApprovalStatus == nameof(ApprovalStatus.Denied))
        {
            await _loginAttemptService.LogAttemptAsync(email, ipAddress, userAgent, false, nameof(FailureReason.AccountDenied));
            return ApiResponse<AuthResponse>.Fail("Your account registration was denied. Please contact support.");
        }

        // Successful login
        await _loginAttemptService.LogAttemptAsync(email, ipAddress, userAgent, true, null);

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        await _sessionService.CreateSessionAsync(
            user.Id, refreshToken, request.DeviceType, request.DeviceName,
            ipAddress, userAgent, expiresAt);

        await _userService.UpdateLastLoginAsync(user.Id);

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            User = MapToUserDto(user)
        }, "Login successful.");
    }

    public async Task<ApiResponse<string>> VerifyOtpAsync(VerifyOtpRequest request)
    {
        _logger.LogInformation("OTP verification attempt for {Email}, type: {OtpType}", request.Email, request.OtpType);

        var isValid = await _otpService.ValidateOtpAsync(request.Email.ToLowerInvariant(), request.OtpCode, request.OtpType);
        if (!isValid)
        {
            return ApiResponse<string>.Fail("Invalid or expired OTP.");
        }

        if (request.OtpType == nameof(OtpType.EmailVerification))
        {
            var user = await _userService.GetByEmailAsync(request.Email.ToLowerInvariant());
            if (user != null)
            {
                await _userService.UpdateEmailVerifiedAsync(user.Id);

                // Send pending approval email instead of welcome email (Phase 2)
                // await _emailService.SendPendingApprovalEmailAsync(user.Email, user.FirstName);

                // Notify admin users about new registration
                await _notificationService.NotifyAdminsOfNewRegistrationAsync(user);

                _logger.LogInformation("Email verified for user {UserId}. Pending admin approval.", user.Id);
            }
        }

        return ApiResponse<string>.Ok("OTP verified successfully. Your account is now pending admin approval.");
    }

    public async Task<ApiResponse<string>> ResendOtpAsync(ResendOtpRequest request)
    {
        _logger.LogInformation("Resend OTP request for {Email}, type: {OtpType}", request.Email, request.OtpType);

        var user = await _userService.GetByEmailAsync(request.Email.ToLowerInvariant());
        if (user == null)
        {
            // Return success even if user doesn't exist to prevent email enumeration
            return ApiResponse<string>.Ok("If the email exists, a new OTP has been sent.");
        }

        var otpCode = await _otpService.GenerateAndStoreOtpAsync(user.Id, user.Email, request.OtpType);

        if (request.OtpType == nameof(OtpType.PasswordReset))
        {
            // await _emailService.SendPasswordResetEmailAsync(user.Email, otpCode);
        }
        else
        {
            // await _emailService.SendOtpEmailAsync(user.Email, otpCode, request.OtpType);
        }

        return ApiResponse<string>.Ok("If the email exists, a new OTP has been sent.");
    }

    public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        _logger.LogInformation("Forgot password request for {Email}", request.Email);

        var user = await _userService.GetByEmailAsync(request.Email.ToLowerInvariant());
        if (user == null)
        {
            // Return success even if user doesn't exist to prevent email enumeration
            return ApiResponse<string>.Ok("If the email exists, a password reset OTP has been sent.");
        }

        var otpCode = await _otpService.GenerateAndStoreOtpAsync(user.Id, user.Email, nameof(OtpType.PasswordReset));
        // await _emailService.SendPasswordResetEmailAsync(user.Email, otpCode);

        return ApiResponse<string>.Ok("If the email exists, a password reset OTP has been sent.");
    }

    public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        _logger.LogInformation("Password reset attempt for {Email}", request.Email);
        var email = request.Email.ToLowerInvariant();

        var isValid = await _otpService.ValidateOtpAsync(email, request.OtpCode, nameof(OtpType.PasswordReset));
        if (!isValid)
        {
            return ApiResponse<string>.Fail("Invalid or expired OTP.");
        }

        var user = await _userService.GetByEmailAsync(email);
        if (user == null)
        {
            return ApiResponse<string>.Fail("User not found.");
        }

        var newPasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _userService.UpdatePasswordAsync(user.Id, newPasswordHash);

        // Revoke all sessions for security
        await _sessionService.RevokeAllUserSessionsAsync(user.Id);

        _logger.LogInformation("Password reset successful for user {UserId}", user.Id);
        return ApiResponse<string>.Ok("Password has been reset successfully. Please log in with your new password.");
    }

    public async Task<ApiResponse<string>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        _logger.LogInformation("Password change attempt for user {UserId}", userId);

        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<string>.Fail("User not found.");
        }

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return ApiResponse<string>.Fail("Current password is incorrect.");
        }

        var newPasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _userService.UpdatePasswordAsync(user.Id, newPasswordHash);

        // Revoke all other sessions for security
        // Note: The current session remains active
        await _sessionService.RevokeAllUserSessionsAsync(user.Id);

        _logger.LogInformation("Password changed successfully for user {UserId}", userId);
        return ApiResponse<string>.Ok("Password changed successfully. Please log in again.");
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        _logger.LogInformation("Token refresh attempt");

        var session = await _sessionService.GetByRefreshTokenAsync(request.RefreshToken);
        if (session == null)
        {
            return ApiResponse<AuthResponse>.Fail("Invalid refresh token.");
        }

        if (session.ExpiresAt < DateTime.UtcNow)
        {
            await _sessionService.RevokeSessionAsync(session.Id);
            return ApiResponse<AuthResponse>.Fail("Refresh token has expired. Please log in again.");
        }

        var user = await _userService.GetByIdAsync(session.UserId);
        if (user == null || !user.IsActive || user.IsDeleted)
        {
            await _sessionService.RevokeSessionAsync(session.Id);
            return ApiResponse<AuthResponse>.Fail("User account is no longer active.");
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        await _sessionService.UpdateLastUsedAsync(session.Id);

        _logger.LogInformation("Token refreshed for user {UserId}", user.Id);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = request.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            User = MapToUserDto(user)
        }, "Token refreshed successfully.");
    }

    public async Task<ApiResponse<string>> LogoutAsync(Guid userId, string refreshToken)
    {
        _logger.LogInformation("Logout attempt for user {UserId}", userId);

        var session = await _sessionService.GetByRefreshTokenAsync(refreshToken);
        if (session != null && session.UserId == userId)
        {
            await _sessionService.RevokeSessionAsync(session.Id);
        }

        _logger.LogInformation("User {UserId} logged out", userId);
        return ApiResponse<string>.Ok("Logged out successfully.");
    }

    public async Task<ApiResponse<CheckApprovalStatusResponse>> CheckApprovalStatusAsync(CheckApprovalStatusRequest request)
    {
        _logger.LogInformation("Approval status check for {Email}", request.Email);
        var email = request.Email.ToLowerInvariant();

        var user = await _userService.GetByEmailAsync(email);

        // Return "Pending" for unknown emails to prevent enumeration
        if (user == null)
        {
            return ApiResponse<CheckApprovalStatusResponse>.Ok(new CheckApprovalStatusResponse
            {
                ApprovalStatus = nameof(ApprovalStatus.Pending),
                Message = "Your account is pending admin approval."
            });
        }

        var message = user.ApprovalStatus switch
        {
            "Approved" => "Your account has been approved. You can now log in.",
            "Denied" => "Your account registration was denied. Please contact support.",
            _ => "Your account is pending admin approval."
        };

        return ApiResponse<CheckApprovalStatusResponse>.Ok(new CheckApprovalStatusResponse
        {
            ApprovalStatus = user.ApprovalStatus,
            Message = message
        });
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            EmailVerified = user.EmailVerified,
            Role = user.Role,
            ApprovalStatus = user.ApprovalStatus,
            ProfilePhotoUrl = user.ProfilePhotoUrl
        };
    }
}
