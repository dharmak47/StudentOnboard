using Dapper;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Implementations;

public class LoginAttemptRepository : ILoginAttemptRepository
{
    private readonly DbConnectionFactory _db;

    public LoginAttemptRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task CreateAsync(LoginAttempt attempt)
    {
        attempt.Id = Guid.NewGuid();
        attempt.AttemptedAt = DateTime.UtcNow;

        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(@"
            INSERT INTO LoginAttempts (Id, Email, IpAddress, UserAgent, IsSuccessful, FailureReason, AttemptedAt)
            VALUES (@Id, @Email, @IpAddress, @UserAgent, @IsSuccessful, @FailureReason, @AttemptedAt)",
            attempt);
    }

    public async Task<int> GetRecentFailedCountAsync(string email, DateTime since)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM LoginAttempts WHERE Email = @Email AND IsSuccessful = @IsSuccessful AND AttemptedAt > @Since",
            new { Email = email, IsSuccessful = false, Since = since });
    }
}
