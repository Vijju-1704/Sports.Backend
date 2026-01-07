using Sports.Domain.Entities;
using Sports.Domain.Enums; // Added

namespace Sports.Application.Interfaces;

public interface INotificationService
{
    Task CreateNotificationAsync(string userId, string message, NotificationType type);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);
    Task MarkAsReadAsync(int notificationId);
}
