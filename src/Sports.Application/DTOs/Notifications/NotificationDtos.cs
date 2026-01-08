namespace Sports.Application.DTOs.Notifications;

public class NotificationDto
{
    public int NotificationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? RelatedEntityId { get; set; } 
    public string? RelatedEntityType { get; set; }
}

public class NotificationPreferencesDto
{
    public bool EmailNotifications { get; set; } = true;
    public bool GameInvitations { get; set; } = true;
    public bool GameUpdates { get; set; } = true;
    public bool GameCancellations { get; set; } = true;
    public bool JoinRequests { get; set; } = true;
}