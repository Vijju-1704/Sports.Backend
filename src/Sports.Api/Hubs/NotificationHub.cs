using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Sports.Api.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// When user connects, add them to their personal notification group
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Each user has their own group for targeted notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} connected to NotificationHub", userId);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// When user disconnects, remove them from their notification group
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} disconnected from NotificationHub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    public async Task MarkAsRead(int notificationId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User {UserId} marked notification {NotificationId} as read", userId, notificationId);
        
        // Notify the client to update UI
        await Clients.Caller.SendAsync("NotificationRead", notificationId);
    }
}

/// <summary>
/// Extension methods for sending notifications via SignalR
/// </summary>
public static class NotificationHubExtensions
{
    /// <summary>
    /// Send a notification to a specific user
    /// </summary>
    public static async Task SendNotificationToUser(
        this IHubContext<NotificationHub> hubContext,
        string userId,
        object notification)
    {
        await hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
    }

    /// <summary>
    /// Send notification to multiple users
    /// </summary>
    public static async Task SendNotificationToUsers(
        this IHubContext<NotificationHub> hubContext,
        IEnumerable<string> userIds,
        object notification)
    {
        var groups = userIds.Select(id => $"user_{id}");
        await hubContext.Clients.Groups(groups).SendAsync("ReceiveNotification", notification);
    }
}
