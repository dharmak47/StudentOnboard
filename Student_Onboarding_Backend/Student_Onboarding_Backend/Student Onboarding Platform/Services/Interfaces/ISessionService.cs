using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Services.Interfaces;

public interface ISessionService
{
    Task<UserSession> CreateSessionAsync(Guid userId, string refreshToken, string? deviceType, string? deviceName, string? ipAddress, string? userAgent, DateTime expiresAt);
    Task<UserSession?> GetByRefreshTokenAsync(string refreshToken);
    Task RevokeSessionAsync(Guid sessionId);
    Task RevokeAllUserSessionsAsync(Guid userId);
    Task RevokeAllExceptAsync(Guid userId, Guid currentSessionId);
    Task UpdateLastUsedAsync(Guid sessionId);
}
