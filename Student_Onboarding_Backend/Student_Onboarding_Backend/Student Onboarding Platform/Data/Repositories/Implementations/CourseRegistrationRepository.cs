using Dapper;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Implementations;

public class CourseRegistrationRepository : ICourseRegistrationRepository
{
    private readonly DbConnectionFactory _db;

    public CourseRegistrationRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<CourseRegistration> CreateAsync(CourseRegistration registration)
    {
        registration.Id = Guid.NewGuid();
        registration.CreatedAt = DateTime.UtcNow;

        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(@"
            INSERT INTO CourseRegistrations (Id, UserId, CourseId, PaymentStatus, PaymentAmount,
                PaymentDate, Notes, IsActive, CreatedAt)
            VALUES (@Id, @UserId, @CourseId, @PaymentStatus, @PaymentAmount,
                @PaymentDate, @Notes, @IsActive, @CreatedAt)",
            registration);

        return registration;
    }

    public async Task<CourseRegistration?> GetByIdAsync(Guid id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<CourseRegistration>(
            "SELECT * FROM CourseRegistrations WHERE Id = @Id AND IsActive = @IsActive",
            new { Id = id, IsActive = true });
    }

    public async Task<IEnumerable<CourseRegistration>> GetByUserIdAsync(Guid userId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CourseRegistration>(
            "SELECT * FROM CourseRegistrations WHERE UserId = @UserId AND IsActive = @IsActive ORDER BY CreatedAt DESC",
            new { UserId = userId, IsActive = true });
    }

    public async Task<CourseRegistration?> GetByUserAndCourseAsync(Guid userId, Guid courseId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<CourseRegistration>(
            "SELECT * FROM CourseRegistrations WHERE UserId = @UserId AND CourseId = @CourseId AND IsActive = @IsActive",
            new { UserId = userId, CourseId = courseId, IsActive = true });
    }

    public async Task<IEnumerable<CourseRegistration>> GetAllAsync(int offset, int pageSize)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CourseRegistration>(@"
            SELECT * FROM CourseRegistrations WHERE IsActive = @IsActive
            ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
            new { IsActive = true, Offset = offset, PageSize = pageSize });
    }

    public async Task<int> GetAllCountAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM CourseRegistrations WHERE IsActive = @IsActive",
            new { IsActive = true });
    }

    public async Task<int> GetCountByUserIdAsync(Guid userId)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM CourseRegistrations WHERE UserId = @UserId AND IsActive = @IsActive",
            new { UserId = userId, IsActive = true });
    }

    public async Task<int> GetActiveCountByCourseAsync(Guid courseId)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM CourseRegistrations WHERE CourseId = @CourseId AND IsActive = @IsActive",
            new { CourseId = courseId, IsActive = true });
    }

    public async Task UpdatePaymentAsync(Guid id, string paymentStatus, decimal? paymentAmount, string? notes)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(@"
            UPDATE CourseRegistrations SET PaymentStatus = @PaymentStatus, PaymentAmount = @PaymentAmount,
                PaymentDate = @PaymentDate, Notes = @Notes, UpdatedAt = @UpdatedAt
            WHERE Id = @Id",
            new
            {
                Id = id,
                PaymentStatus = paymentStatus,
                PaymentAmount = paymentAmount,
                PaymentDate = paymentStatus == "Paid" ? DateTime.UtcNow : (DateTime?)null,
                Notes = notes,
                UpdatedAt = DateTime.UtcNow
            });
    }

    public async Task UpdateCompletionAsync(Guid id, bool isCompleted, DateTime? completedAt)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(@"
            UPDATE CourseRegistrations SET IsCompleted = @IsCompleted, CompletedAt = @CompletedAt, UpdatedAt = @UpdatedAt
            WHERE Id = @Id",
            new
            {
                Id = id,
                IsCompleted = isCompleted,
                CompletedAt = completedAt,
                UpdatedAt = DateTime.UtcNow
            });
    }
}
