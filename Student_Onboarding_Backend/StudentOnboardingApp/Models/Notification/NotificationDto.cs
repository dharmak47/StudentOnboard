using System.Text.Json.Serialization;

namespace StudentOnboardingApp.Models.Notification;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    [JsonPropertyName("message")]
    public string Body { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Returns CreatedAt converted from UTC to local time.
    /// </summary>
    [JsonIgnore]
    public DateTime CreatedAtLocal => CreatedAt.Kind == DateTimeKind.Utc
        ? CreatedAt.ToLocalTime()
        : DateTime.SpecifyKind(CreatedAt, DateTimeKind.Utc).ToLocalTime();

    [JsonIgnore]
    public string TimeAgo
    {
        get
        {
            var diff = DateTime.Now - CreatedAtLocal;
            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
            return CreatedAtLocal.ToString("MMM dd, yyyy");
        }
    }
}
