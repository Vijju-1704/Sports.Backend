using Microsoft.EntityFrameworkCore;
using Sports.Application.DTOs.Notifications;
using Sports.Application.Interfaces;
using Sports.Domain.Entities;
using Sports.Domain.Enums;
using Sports.Domain.Interfaces;

namespace Sports.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork Uow;

    public NotificationService(IUnitOfWork uow)
    {
        Uow = uow;
    }

    public async Task CreateNotificationAsync(
        string userId,
        string message,
        NotificationType type,
        int? relatedEntityId = null,
        string? relatedEntityType = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            RelatedGameId = relatedEntityId // Save the game ID for rating notifications
        };

        await Uow.Repository<Notification>().AddAsync(notification);
        await Uow.SaveChangesAsync();
        
        // Note: SignalR notifications are handled at the API layer
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId, bool unreadOnly = false)
    {
        var query = Uow.Repository<Notification>()
            .GetQueryable()
            .Where(n => n.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(50) // Limit to latest 50
            .ToListAsync();

        return notifications.Select(n => new NotificationDto
        {
            NotificationId = n.NotificationId,
            UserId = n.UserId,
            Message = n.Message,
            Type = n.Type.ToString(),
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            RelatedEntityId = n.RelatedGameId,
            RelatedEntityType = n.RelatedGameId.HasValue ? "Game" : null
        });
    }

    public async Task<bool> MarkAsReadAsync(int notificationId)
    {
        var notification = await Uow.Repository<Notification>().GetByIdAsync(notificationId);
        if (notification == null) return false;

        notification.IsRead = true;
        await Uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(string userId)
    {
        var notifications = await Uow.Repository<Notification>()
            .FindAsync(n => n.UserId == userId && !n.IsRead);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }
        await Uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteNotificationAsync(int notificationId, string userId)
    {
        var notification = await Uow.Repository<Notification>().GetByIdAsync(notificationId);
        if (notification == null || notification.UserId != userId) return false;

        Uow.Repository<Notification>().Remove(notification);
        await Uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAllNotificationsAsync(string userId)
    {
        var notifications = await Uow.Repository<Notification>()
            .FindAsync(n => n.UserId == userId);

        foreach (var notification in notifications)
        {
            Uow.Repository<Notification>().Remove(notification);
        }

        await Uow.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        var count = await Uow.Repository<Notification>()
            .GetQueryable()
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        return count;
    }
}

