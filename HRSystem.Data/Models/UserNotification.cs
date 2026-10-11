using HRSystem.Common.Enums;

namespace HRSystem.Data.Models;

public class UserNotification
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public NotificationKind Kind { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? LinkUrl { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}
