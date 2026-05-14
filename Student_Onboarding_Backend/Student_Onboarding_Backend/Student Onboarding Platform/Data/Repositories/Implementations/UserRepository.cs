using Dapper;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _db;

    public UserRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Id = @Id AND IsDeleted = @IsDeleted",
            new { Id = id, IsDeleted = false });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            using var conn = _db.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Email = @Email AND IsDeleted = @IsDeleted",
                new { Email = email, IsDeleted = false });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE PhoneNumber = @PhoneNumber AND IsDeleted = @IsDeleted",
            new { PhoneNumber = phoneNumber, IsDeleted = false });
    }

    public async Task<User> CreateAsync(User user)
    {
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;

        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(@"
            INSERT INTO Users (Id, FirstName, LastName, Email, PhoneNumber, PasswordHash,
                EmailVerified, PhoneVerified, IsActive, IsDeleted, Role, ApprovalStatus, CreatedAt)
            VALUES (@Id, @FirstName, @LastName, @Email, @PhoneNumber, @PasswordHash,
                @EmailVerified, @PhoneVerified, @IsActive, @IsDeleted, @Role, @ApprovalStatus, @CreatedAt)",
            user);

        return user;
    }

    public async Task UpdateEmailVerifiedAsync(Guid userId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Users SET EmailVerified = @EmailVerified, UpdatedAt = @UpdatedAt WHERE Id = @Id",
            new { Id = userId, EmailVerified = true, UpdatedAt = DateTime.UtcNow });
    }

    public async Task UpdatePasswordAsync(Guid userId, string passwordHash, DateTime passwordUpdatedAt)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Users SET PasswordHash = @PasswordHash, PasswordUpdatedAt = @PasswordUpdatedAt, UpdatedAt = @UpdatedAt WHERE Id = @Id",
            new { Id = userId, PasswordHash = passwordHash, PasswordUpdatedAt = passwordUpdatedAt, UpdatedAt = DateTime.UtcNow });
    }

    public async Task UpdateLastLoginAsync(Guid userId, DateTime lastLoginAt)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Users SET LastLoginAt = @LastLoginAt WHERE Id = @Id",
            new { Id = userId, LastLoginAt = lastLoginAt });
    }

    public async Task UpdateApprovalStatusAsync(Guid userId, string status, Guid? approvedBy, string? denialReason)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(@"
            UPDATE Users SET ApprovalStatus = @Status, ApprovedBy = @ApprovedBy,
                ApprovedAt = @ApprovedAt, DenialReason = @DenialReason, UpdatedAt = @UpdatedAt
            WHERE Id = @Id",
            new
            {
                Id = userId,
                Status = status,
                ApprovedBy = approvedBy,
                ApprovedAt = DateTime.UtcNow,
                DenialReason = denialReason,
                UpdatedAt = DateTime.UtcNow
            });
    }

    public async Task<IEnumerable<User>> GetStudentsAsync(int offset, int pageSize, string? approvalStatus, string? search)
    {
        using var conn = _db.CreateConnection();
        var sql = @"SELECT * FROM Users WHERE Role = @Role AND IsDeleted = @IsDeleted";

        if (!string.IsNullOrEmpty(approvalStatus))
            sql += " AND ApprovalStatus = @ApprovalStatus";

        if (!string.IsNullOrEmpty(search))
            sql += " AND (FirstName LIKE @Search OR LastName LIKE @Search OR Email LIKE @Search)";

        sql += " ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        return await conn.QueryAsync<User>(sql, new
        {
            Role = "Student",
            IsDeleted = false,
            ApprovalStatus = approvalStatus,
            Search = $"%{search}%",
            Offset = offset,
            PageSize = pageSize
        });
    }

    public async Task<int> GetStudentsCountAsync(string? approvalStatus, string? search)
    {
        using var conn = _db.CreateConnection();
        var sql = @"SELECT COUNT(*) FROM Users WHERE Role = @Role AND IsDeleted = @IsDeleted";

        if (!string.IsNullOrEmpty(approvalStatus))
            sql += " AND ApprovalStatus = @ApprovalStatus";

        if (!string.IsNullOrEmpty(search))
            sql += " AND (FirstName LIKE @Search OR LastName LIKE @Search OR Email LIKE @Search)";

        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            Role = "Student",
            IsDeleted = false,
            ApprovalStatus = approvalStatus,
            Search = $"%{search}%"
        });
    }

    public async Task<IEnumerable<User>> GetAdminUsersAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<User>(
            "SELECT * FROM Users WHERE Role = @Role AND IsDeleted = @IsDeleted AND IsActive = @IsActive",
            new { Role = "Admin", IsDeleted = false, IsActive = true });
    }

    public async Task UpdateProfileAsync(Guid userId, string firstName, string lastName, string? phoneNumber,
        DateTime? dateOfBirth, string? address, string? education)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(@"
            UPDATE Users SET FirstName = @FirstName, LastName = @LastName,
                PhoneNumber = @PhoneNumber, DateOfBirth = @DateOfBirth,
                Address = @Address, Education = @Education, UpdatedAt = @UpdatedAt
            WHERE Id = @Id",
            new { Id = userId, FirstName = firstName, LastName = lastName, PhoneNumber = phoneNumber,
                DateOfBirth = dateOfBirth, Address = address, Education = education, UpdatedAt = DateTime.UtcNow });
    }

    public async Task UpdateProfilePhotoAsync(Guid userId, string photoUrl)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Users SET ProfilePhotoUrl = @PhotoUrl, UpdatedAt = @UpdatedAt WHERE Id = @Id",
            new { Id = userId, PhotoUrl = photoUrl, UpdatedAt = DateTime.UtcNow });
    }

    public async Task UpdateIsActiveAsync(Guid userId, bool isActive)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Users SET IsActive = @IsActive, UpdatedAt = @UpdatedAt WHERE Id = @Id",
            new { Id = userId, IsActive = isActive, UpdatedAt = DateTime.UtcNow });
    }

    public async Task<IEnumerable<User>> GetStudentsWithBirthdayTodayAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<User>(@"
            SELECT * FROM Users
            WHERE Role = 'Student' AND IsDeleted = 0 AND IsActive = 1
              AND ApprovalStatus = 'Approved'
              AND DateOfBirth IS NOT NULL
              AND MONTH(DateOfBirth) = MONTH(GETDATE())
              AND DAY(DateOfBirth) = DAY(GETDATE())",
            new { });
    }
}
