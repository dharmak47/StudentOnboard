using Dapper;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Implementations;

public class OtpRepository : IOtpRepository
{
    private readonly DbConnectionFactory _db;

    public OtpRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<OtpVerification> CreateAsync(OtpVerification otp)
    {
        otp.Id = Guid.NewGuid();
        otp.CreatedAt = DateTime.UtcNow;

        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(@"
            INSERT INTO OtpVerifications (Id, UserId, Email, PhoneNumber, OtpCode, OtpType,
                AttemptCount, MaxAttempts, ExpiresAt, IsUsed, CreatedAt)
            VALUES (@Id, @UserId, @Email, @PhoneNumber, @OtpCode, @OtpType,
                @AttemptCount, @MaxAttempts, @ExpiresAt, @IsUsed, @CreatedAt)",
            otp);

        return otp;
    }

    public async Task<OtpVerification?> GetLatestValidAsync(string email, string otpType)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<OtpVerification>(@"
            SELECT * FROM OtpVerifications
            WHERE Email = @Email AND OtpType = @OtpType AND IsUsed = @IsUsed AND ExpiresAt > @Now
            ORDER BY CreatedAt DESC",
            new { Email = email, OtpType = otpType, IsUsed = false, Now = DateTime.UtcNow });
    }

    public async Task IncrementAttemptCountAsync(Guid otpId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE OtpVerifications SET AttemptCount = AttemptCount + 1 WHERE Id = @Id",
            new { Id = otpId });
    }

    public async Task MarkAsUsedAsync(Guid otpId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE OtpVerifications SET IsUsed = @IsUsed WHERE Id = @Id",
            new { Id = otpId, IsUsed = true });
    }

    public async Task InvalidateByEmailAndTypeAsync(string email, string otpType)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE OtpVerifications SET IsUsed = @IsUsed WHERE Email = @Email AND OtpType = @OtpType AND IsUsed = @CurrentState",
            new { Email = email, OtpType = otpType, IsUsed = true, CurrentState = false });
    }
}
