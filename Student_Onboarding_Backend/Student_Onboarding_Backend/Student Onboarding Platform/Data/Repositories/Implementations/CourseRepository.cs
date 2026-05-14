using Dapper;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Implementations;

public class CourseRepository : ICourseRepository
{
    private readonly DbConnectionFactory _db;

    public CourseRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<Course> CreateAsync(Course course)
    {
        course.Id = Guid.NewGuid();
        course.CreatedAt = DateTime.UtcNow;

        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(@"
            INSERT INTO Courses (Id, Name, Description, Fees, OfferPrice, Syllabus, Duration,
                Instructor, Category, Thumbnail, IsActive, IsDeleted, CreatedBy, CreatedAt)
            VALUES (@Id, @Name, @Description, @Fees, @OfferPrice, @Syllabus, @Duration,
                @Instructor, @Category, @Thumbnail, @IsActive, @IsDeleted, @CreatedBy, @CreatedAt)",
            course);

        return course;
    }

    public async Task<Course?> GetByIdAsync(Guid id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Course>(
            "SELECT * FROM Courses WHERE Id = @Id AND IsDeleted = @IsDeleted",
            new { Id = id, IsDeleted = false });
    }

    public async Task<IEnumerable<Course>> GetActiveCoursesAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Course>(
            "SELECT * FROM Courses WHERE IsActive = @IsActive AND IsDeleted = @IsDeleted ORDER BY CreatedAt DESC",
            new { IsActive = true, IsDeleted = false });
    }

    public async Task UpdateAsync(Course course)
    {
        course.UpdatedAt = DateTime.UtcNow;

        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(@"
            UPDATE Courses SET Name = @Name, Description = @Description, Fees = @Fees,
                OfferPrice = @OfferPrice, Syllabus = @Syllabus, Duration = @Duration,
                Instructor = @Instructor, Category = @Category, Thumbnail = @Thumbnail,
                IsActive = @IsActive, UpdatedAt = @UpdatedAt
            WHERE Id = @Id",
            course);
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Courses SET IsDeleted = @IsDeleted, UpdatedAt = @UpdatedAt WHERE Id = @Id",
            new { Id = id, IsDeleted = true, UpdatedAt = DateTime.UtcNow });
    }
}
