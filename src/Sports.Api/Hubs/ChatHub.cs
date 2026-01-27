using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Sports.Application.Interfaces;
using Sports.Domain.Entities;
using Sports.Application.DTOs.Chat;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Sports.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatHub> Logger;
    private readonly INotificationService _notificationService;

    public ChatHub(IChatService chatService, ILogger<ChatHub> logger, INotificationService notificationService)
    {
        _chatService = chatService;
        Logger = logger;
        _notificationService = notificationService;
    }

    public async Task JoinGameChat(string gameId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        Logger.LogInformation("User {UserId} joined chat group {GameId}", Context.UserIdentifier, gameId);
    }

    public async Task LeaveGameChat(string gameId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
        Logger.LogInformation("User {UserId} left chat group {GameId}", Context.UserIdentifier, gameId);
    }

    public async Task SendMessage(string gameId, string content)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return;

            // Save message
            var message = await _chatService.SendMessageAsync(int.Parse(gameId), userId, new SendMessageDto { Content = content });

            // Broadcast to group
            await Clients.Group(gameId).SendAsync("ReceiveMessage", new 
            {
                Id = message.MessageId,
                SenderId = message.SenderUserId,
                Content = message.Content,
                Timestamp = message.Timestamp,
                IsCurrentUser = false
            });

            // Handle Mentions: Look for @username patterns
            var mentions = Regex.Matches(content, @"@(\w+)");
            foreach (Match match in mentions)
            {
                var username = match.Groups[1].Value;
                Logger.LogInformation("User {UserId} mentioned {MentionedUser}", userId, username);
            }

            Logger.LogInformation("Message sent in game {GameId} by user {UserId}", gameId, userId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error sending message in game {GameId}", gameId);
            await Clients.Caller.SendAsync("Error", "Failed to send message");
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Logger.LogInformation("User {UserId} connected to ChatHub", userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Logger.LogInformation("User {UserId} disconnected from ChatHub", userId);
        await base.OnDisconnectedAsync(exception);
    }
}

