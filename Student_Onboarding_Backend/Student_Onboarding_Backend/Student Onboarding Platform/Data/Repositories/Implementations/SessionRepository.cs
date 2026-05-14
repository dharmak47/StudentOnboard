using Dapper;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Implementations;

public class SessionRepository : ISessionRepository
{
    private readonly DbConnectionFactory _db;

    public SessionRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<UserSession> CreateAsync(UserSession session)
    {
        session.Id = Guid.NewGuid();
        session.CreatedAt = DateTime.UtcNow;

        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(@"
            INSERT INTO UserSessions (Id, UserId, RefreshToken, DeviceType, DeviceName,
                IpAddress, UserAgent, ExpiresAt, IsRevoked, CreatedAt)
            VALUES (@Id, @UserId, @RefreshToken, @DeviceType, @DeviceName,
                @IpAddress, @UserAgent, @ExpiresAt, @IsRevoked, @CreatedAt)",
            session);

        return session;
    }

    public async Task<UserSession?> GetByRefreshTokenAsync(string refreshToken)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<UserSession>(
            "SELECT * FROM UserSessions WHERE RefreshToken = @RefreshToken AND IsRevoked = @IsRevoked",
            new { RefreshToken = refreshToken, IsRevoked = false });
    }

    public async Task RevokeAsync(Guid sessionId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE UserSessions SET IsRevoked = @IsRevoked WHERE Id = @Id",
            new { Id = sessionId, IsRevoked = true });
    }

    public async Task RevokeAllByUserIdAsync(Guid userId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE UserSessions SET IsRevoked = @IsRevoked WHERE UserId = @UserId AND IsRevoked = @CurrentState",
            new { UserId = userId, IsRevoked = true, CurrentState = false });
    }

    public async Task RevokeAllExceptAsync(Guid userId, Guid currentSessionId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE UserSessions SET IsRevoked = @IsRevoked WHERE UserId = @UserId AND Id != @CurrentSessionId AND IsRevoked = @CurrentState",
            new { UserId = userId, CurrentSessionId = currentSessionId, IsRevoked = true, CurrentState = false });
    }

    public async Task UpdateLastUsedAsync(Guid sessionId, DateTime lastUsedAt)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE UserSessions SET LastUsedAt = @LastUsedAt WHERE Id = @Id",
            new { Id = sessionId, LastUsedAt = lastUsedAt });
    }
}
