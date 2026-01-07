using Sports.Application.Interfaces;
using Sports.Domain.Entities;
using Sports.Domain.Interfaces;
using Sports.Domain.Enums; // Added

namespace Sports.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _uow;

    public NotificationService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task CreateNotificationAsync(string userId, string message, NotificationType type)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            Type = type,
            // RelatedEntityId removed as it doesn't exist in Entity
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Repository<Notification>().AddAsync(notification);
        await _uow.SaveChangesAsync();
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
    {
        var notifications = await _uow.Repository<Notification>()
            .FindAsync(n => n.UserId == userId);
        
        return notifications.OrderByDescending(n => n.CreatedAt);
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _uow.Repository<Notification>().GetByIdAsync(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            await _uow.SaveChangesAsync();
        }
    }
}
