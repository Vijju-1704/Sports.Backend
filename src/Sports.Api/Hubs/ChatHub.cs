using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Sports.Application.Interfaces;
using Sports.Domain.Constants;

namespace Sports.Api.Hubs;


[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService ChatService;
    private readonly ILogger<ChatHub> Logger;

    public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
    {
        ChatService = chatService;
        Logger = logger;
    }
    
    public async Task JoinGameRoom(int gameId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"game_{gameId}");
        Logger.LogInformation("User {UserId} joined game room {GameId}", userId, gameId);
        
        // Notify others that user joined
        await Clients.Group($"game_{gameId}").SendAsync("UserJoined", new
        {
            UserId = userId,
            Message = MessageStrings.UserJoinedChatMessage
        });
    }

    public async Task LeaveGameRoom(int gameId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"game_{gameId}");
        Logger.LogInformation(MessageStrings.UserLeftGameRoomLog, userId, gameId);
    }

    
    public async Task SendMessage(int gameId, string content)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            Logger.LogWarning(MessageStrings.UnauthorizedSendMessageAttempt);
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
            var message = await ChatService.SendMessageAsync(gameId, userId, dto);

            // Broadcast to all clients in the game room
            await Clients.Group($"game_{gameId}").SendAsync("ReceiveMessage", new
            {
                MessageId = message.MessageId,
                GameId = gameId,
                SenderId = userId,
                SenderName = message.SenderName,
                Content = message.Content,
                Timestamp = message.Timestamp,
                IsCurrentUser = false 
            });

            Logger.LogInformation(MessageStrings.MessageSentLog, gameId, userId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, MessageStrings.ErrorSendingMessageLog, gameId);
            await Clients.Caller.SendAsync("Error", MessageStrings.FailedToSendMessage);
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Logger.LogInformation(MessageStrings.UserConnectedToChatHub, userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Logger.LogInformation(MessageStrings.UserDisconnectedFromChatHub, userId);
        await base.OnDisconnectedAsync(exception);
    }
}
