using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Interfaces;

public interface ISessionRepository
{
    Task<UserSession> CreateAsync(UserSession session);
    Task<UserSession?> GetByRefreshTokenAsync(string refreshToken);
    Task RevokeAsync(Guid sessionId);
    Task RevokeAllByUserIdAsync(Guid userId);
    Task RevokeAllExceptAsync(Guid userId, Guid currentSessionId);
    Task UpdateLastUsedAsync(Guid sessionId, DateTime lastUsedAt);
}
