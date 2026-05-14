using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.Entities;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Services.Implementations;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ILogger<SessionService> _logger;

    public SessionService(ISessionRepository sessionRepository, ILogger<SessionService> logger)
    {
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    public async Task<UserSession> CreateSessionAsync(Guid userId, string refreshToken, string? deviceType, string? deviceName, string? ipAddress, string? userAgent, DateTime expiresAt)
    {
        var session = new UserSession
        {
            UserId = userId,
            RefreshToken = refreshToken,
            DeviceType = deviceType,
            DeviceName = deviceName,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ExpiresAt = expiresAt,
            IsRevoked = false
        };

        var created = await _sessionRepository.CreateAsync(session);
        _logger.LogInformation("Session created for user {UserId}, device: {DeviceType}", userId, deviceType ?? "Unknown");
        return created;
    }

    public async Task<UserSession?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _sessionRepository.GetByRefreshTokenAsync(refreshToken);
    }

    public async Task RevokeSessionAsync(Guid sessionId)
    {
        await _sessionRepository.RevokeAsync(sessionId);
        _logger.LogInformation("Session {SessionId} revoked", sessionId);
    }

    public async Task RevokeAllUserSessionsAsync(Guid userId)
    {
        await _sessionRepository.RevokeAllByUserIdAsync(userId);
        _logger.LogInformation("All sessions revoked for user {UserId}", userId);
    }

    public async Task RevokeAllExceptAsync(Guid userId, Guid currentSessionId)
    {
        await _sessionRepository.RevokeAllExceptAsync(userId, currentSessionId);
        _logger.LogInformation("All sessions except {SessionId} revoked for user {UserId}", currentSessionId, userId);
    }

    public async Task UpdateLastUsedAsync(Guid sessionId)
    {
        await _sessionRepository.UpdateLastUsedAsync(sessionId, DateTime.UtcNow);
    }
}
