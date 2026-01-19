using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Sports.Application.Interfaces;

namespace Sports.Api.Hubs;

/// <summary>
/// SignalR Hub for real-time game chat
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// Join a game's chat room
    /// </summary>
    public async Task JoinGameRoom(int gameId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"game_{gameId}");
        _logger.LogInformation("User {UserId} joined game room {GameId}", userId, gameId);
        
        // Notify others that user joined
        await Clients.Group($"game_{gameId}").SendAsync("UserJoined", new
        {
            UserId = userId,
            Message = "A user joined the chat"
        });
    }

    /// <summary>
    /// Leave a game's chat room
    /// </summary>
    public async Task LeaveGameRoom(int gameId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"game_{gameId}");
        _logger.LogInformation("User {UserId} left game room {GameId}", userId, gameId);
    }

    /// <summary>
    /// Send a message to game chat
    /// </summary>
    public async Task SendMessage(int gameId, string content)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Unauthorized send message attempt");
            return;
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        try
        {
            // Save to database
            var dto = new Sports.Application.DTOs.Chat.SendMessageDto { Content = content };
            var message = await _chatService.SendMessageAsync(gameId, userId, dto);

            // Broadcast to all clients in the game room
            await Clients.Group($"game_{gameId}").SendAsync("ReceiveMessage", new
            {
                MessageId = message.MessageId,
                GameId = gameId,
                SenderId = userId,
                SenderName = message.SenderName,
                Content = message.Content,
                Timestamp = message.Timestamp,
                IsCurrentUser = false // Client will determine this
            });

            _logger.LogInformation("Message sent in game {GameId} by user {UserId}", gameId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message in game {GameId}", gameId);
            await Clients.Caller.SendAsync("Error", "Failed to send message");
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User {UserId} connected to ChatHub", userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User {UserId} disconnected from ChatHub", userId);
        await base.OnDisconnectedAsync(exception);
    }
}
