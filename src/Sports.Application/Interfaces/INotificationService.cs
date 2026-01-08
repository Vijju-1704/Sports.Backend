using Sports.Application.DTOs.Notifications;
using Sports.Domain.Entities;
using Sports.Domain.Enums; 

namespace Sports.Application.Interfaces;

public interface INotificationService
{
    Task CreateNotificationAsync(string userId, string message, NotificationType type, int? relatedEntityId = null, string? relatedEntityType = null);
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
    Task<bool> MarkAsReadAsync(int notificationId);
    Task<bool> MarkAllAsReadAsync(string userId);
    Task<bool> DeleteNotificationAsync(int notificationId, string userId);
    Task<bool> DeleteAllNotificationsAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
}
