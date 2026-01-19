using Microsoft.EntityFrameworkCore;
using Sports.Application.DTOs.Notifications;
using Sports.Application.Interfaces;
using Sports.Domain.Entities;
using Sports.Domain.Enums;
using Sports.Domain.Interfaces;

namespace Sports.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _uow;

    public NotificationService(IUnitOfWork uow)
    {
        _uow = uow;
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
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Repository<Notification>().AddAsync(notification);
        await _uow.SaveChangesAsync();
        
        // Note: SignalR notifications are handled at the API layer
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId, bool unreadOnly = false)
    {
        var query = _uow.Repository<Notification>()
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
            CreatedAt = n.CreatedAt
        });
    }

    public async Task<bool> MarkAsReadAsync(int notificationId)
    {
        var notification = await _uow.Repository<Notification>().GetByIdAsync(notificationId);
        if (notification == null) return false;

        notification.IsRead = true;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(string userId)
    {
        var notifications = await _uow.Repository<Notification>()
            .FindAsync(n => n.UserId == userId && !n.IsRead);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteNotificationAsync(int notificationId, string userId)
    {
        var notification = await _uow.Repository<Notification>().GetByIdAsync(notificationId);
        if (notification == null || notification.UserId != userId) return false;

        _uow.Repository<Notification>().Remove(notification);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAllNotificationsAsync(string userId)
    {
        var notifications = await _uow.Repository<Notification>()
            .FindAsync(n => n.UserId == userId);

        foreach (var notification in notifications)
        {
            _uow.Repository<Notification>().Remove(notification);
        }

        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        var count = await _uow.Repository<Notification>()
            .GetQueryable()
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        return count;
    }
}

