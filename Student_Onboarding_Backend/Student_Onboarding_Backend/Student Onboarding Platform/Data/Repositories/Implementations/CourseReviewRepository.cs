using Dapper;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Implementations;

public class CourseReviewRepository : ICourseReviewRepository
{
    private readonly DbConnectionFactory _db;

    public CourseReviewRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<CourseReview> CreateAsync(CourseReview review)
    {
        review.Id = Guid.NewGuid();
        review.CreatedAt = DateTime.UtcNow;

        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(@"
            INSERT INTO CourseReviews (Id, CourseId, UserId, Rating, Remarks, CreatedAt)
            VALUES (@Id, @CourseId, @UserId, @Rating, @Remarks, @CreatedAt)",
            review);

        return review;
    }

    public async Task<CourseReview?> GetByUserAndCourseAsync(Guid userId, Guid courseId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<CourseReview>(
            "SELECT * FROM CourseReviews WHERE UserId = @UserId AND CourseId = @CourseId",
            new { UserId = userId, CourseId = courseId });
    }

    public async Task<IEnumerable<CourseReview>> GetByCourseIdAsync(Guid courseId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<CourseReview>(
            "SELECT * FROM CourseReviews WHERE CourseId = @CourseId ORDER BY CreatedAt DESC",
            new { CourseId = courseId });
    }

    public async Task<double?> GetAverageRatingAsync(Guid courseId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<double?>(
            "SELECT AVG(CAST(Rating AS FLOAT)) FROM CourseReviews WHERE CourseId = @CourseId",
            new { CourseId = courseId });
    }

    public async Task<int> GetReviewCountAsync(Guid courseId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<int>(
            "SELECT COUNT(*) FROM CourseReviews WHERE CourseId = @CourseId",
            new { CourseId = courseId });
    }
}
