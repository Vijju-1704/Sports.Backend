using Microsoft.AspNetCore.Identity;
using Sports.Application.DTOs.Chat;
using Sports.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Sports.Domain.Entities;
using Sports.Domain.Interfaces;
using Sports.Infrastructure.Identity;

public class ChatService : IChatService
{
    private readonly IUnitOfWork Uow;
    private readonly UserManager<ApplicationUser> UserManager;

    public ChatService(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        Uow = uow;
        UserManager = userManager;
    }

    public async Task<IEnumerable<ChatMessageDto>> GetGameMessagesAsync(int gameId, string currentUserId)
    {
        var messages = await Uow.Repository<ChatMessage>()
            .GetQueryable()
            .Where(m => m.GameId == gameId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        var messageDtos = new List<ChatMessageDto>();

        foreach (var msg in messages)
        {
            var sender = await UserManager.FindByIdAsync(msg.SenderUserId);
            messageDtos.Add(new ChatMessageDto
            {
                MessageId = msg.MessageId,
                GameId = msg.GameId,
                SenderUserId = msg.SenderUserId,
                SenderName = sender?.FullName ?? "Unknown",
                Content = msg.Content,
                Timestamp = msg.Timestamp,
                IsCurrentUser = msg.SenderUserId == currentUserId
            });
        }

        return messageDtos;
    }

    public async Task<ChatMessageDto> SendMessageAsync(int gameId, string userId, SendMessageDto dto)
    {
        // Verify user is a participant
        var participant = await Uow.Repository<GameParticipant>()
            .FindAsync(p => p.GameId == gameId && p.UserId == userId);

        if (!participant.Any())
        {
            throw new UnauthorizedAccessException("You must be a participant to send messages");
        }

        var message = new ChatMessage
        {
            GameId = gameId,
            SenderUserId = userId,
            Content = dto.Content,
            Timestamp = DateTime.UtcNow
        };

        await Uow.Repository<ChatMessage>().AddAsync(message);
        await Uow.SaveChangesAsync();

        var sender = await UserManager.FindByIdAsync(userId);

        return new ChatMessageDto
        {
            MessageId = message.MessageId,
            GameId = message.GameId,
            SenderUserId = message.SenderUserId,
            SenderName = sender?.FullName ?? "Unknown",
            Content = message.Content,
            Timestamp = message.Timestamp,
            IsCurrentUser = true
        };
    }

    public async Task<bool> DeleteMessageAsync(int messageId, string userId)
    {
        var message = await Uow.Repository<ChatMessage>().GetByIdAsync(messageId);
        if (message == null) return false;

        // Only sender can delete their message
        if (message.SenderUserId != userId) return false;

        Uow.Repository<ChatMessage>().Remove(message);
        await Uow.SaveChangesAsync();
        return true;
    }
}