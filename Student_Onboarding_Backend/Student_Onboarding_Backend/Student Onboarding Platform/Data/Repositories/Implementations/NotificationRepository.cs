using Dapper;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Implementations;

public class NotificationRepository : INotificationRepository
{
    private readonly DbConnectionFactory _db;

    public NotificationRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task CreateAsync(Notification notification)
    {
        notification.Id = Guid.NewGuid();
        notification.CreatedAt = DateTime.UtcNow;

        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(@"
            INSERT INTO Notifications (Id, UserId, Type, Title, Message, ReferenceId, IsRead, CreatedAt)
            VALUES (@Id, @UserId, @Type, @Title, @Message, @ReferenceId, @IsRead, @CreatedAt)",
            notification);
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Notification>(
            "SELECT * FROM Notifications WHERE UserId = @UserId ORDER BY CreatedAt DESC",
            new { UserId = userId });
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Notifications WHERE UserId = @UserId AND IsRead = @IsRead",
            new { UserId = userId, IsRead = false });
    }

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE Notifications SET IsRead = @IsRead WHERE Id = @Id",
            new { Id = notificationId, IsRead = true });
    }
}
