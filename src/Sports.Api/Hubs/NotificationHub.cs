using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Sports.Domain.Constants;

namespace Sports.Api.Hubs;


[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> Logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        Logger = logger;
    }

   
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Each user has their own group for targeted notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            Logger.LogInformation(MessageStrings.UserConnectedToNotificationHub, userId);
        }

        await base.OnConnectedAsync();
    }

   
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            Logger.LogInformation(MessageStrings.UserDisconnectedFromNotificationHub, userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

 
    public async Task MarkAsRead(int notificationId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Logger.LogInformation(MessageStrings.UserMarkedNotificationAsRead, userId, notificationId);
        
        // Notify the client to update UI
        await Clients.Caller.SendAsync(MessageStrings.SignalRNotificationReadEvent, notificationId);
    }
}


public static class NotificationHubExtensions
{
  
    public static async Task SendNotificationToUser(
        this IHubContext<NotificationHub> hubContext,
        string userId,
        object notification)
    {
        await hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
    }

    
    public static async Task SendNotificationToUsers(
        this IHubContext<NotificationHub> hubContext,
        IEnumerable<string> userIds,
        object notification)
    {
        var groups = userIds.Select(id => $"user_{id}");
        await hubContext.Clients.Groups(groups).SendAsync(MessageStrings.SignalRReceiveNotificationEvent,notification);
    }
}
