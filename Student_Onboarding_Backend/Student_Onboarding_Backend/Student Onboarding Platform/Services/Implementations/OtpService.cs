using Microsoft.Extensions.Options;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Helpers;
using Student_Onboarding_Platform.Models.Entities;
using Student_Onboarding_Platform.Models.Settings;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Services.Implementations;

public class OtpService : IOtpService
{
    private readonly IOtpRepository _otpRepository;
    private readonly OtpSettings _otpSettings;
    private readonly ILogger<OtpService> _logger;

    public OtpService(IOtpRepository otpRepository, IOptions<OtpSettings> otpSettings, ILogger<OtpService> logger)
    {
        _otpRepository = otpRepository;
        _otpSettings = otpSettings.Value;
        _logger = logger;
    }

    public async Task<string> GenerateAndStoreOtpAsync(Guid? userId, string email, string otpType)
    {
        // Invalidate any existing OTPs for this email and type
        await _otpRepository.InvalidateByEmailAndTypeAsync(email, otpType);

        var otpCode = OtpGenerator.Generate(_otpSettings.Length);

        var otp = new OtpVerification
        {
            UserId = userId,
            Email = email,
            OtpCode = otpCode,
            OtpType = otpType,
            AttemptCount = 0,
            MaxAttempts = _otpSettings.MaxAttempts,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_otpSettings.ExpirationMinutes),
            IsUsed = false
        };

        await _otpRepository.CreateAsync(otp);
        _logger.LogInformation("OTP generated for {Email}, type: {OtpType}", email, otpType);

        return otpCode;
    }

    public async Task<bool> ValidateOtpAsync(string email, string otpCode, string otpType)
    {
        var otp = await _otpRepository.GetLatestValidAsync(email, otpType);

        if (otp == null)
        {
            _logger.LogWarning("No valid OTP found for {Email}, type: {OtpType}", email, otpType);
            return false;
        }

        if (otp.AttemptCount >= otp.MaxAttempts)
        {
            _logger.LogWarning("OTP max attempts reached for {Email}, type: {OtpType}", email, otpType);
            await _otpRepository.MarkAsUsedAsync(otp.Id);
            return false;
        }

        await _otpRepository.IncrementAttemptCountAsync(otp.Id);

        if (otp.OtpCode != otpCode)
        {
            _logger.LogWarning("Invalid OTP entered for {Email}, type: {OtpType}", email, otpType);
            return false;
        }

        await _otpRepository.MarkAsUsedAsync(otp.Id);
        _logger.LogInformation("OTP verified successfully for {Email}, type: {OtpType}", email, otpType);
        return true;
    }
}
