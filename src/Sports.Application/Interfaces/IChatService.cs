using Sports.Application.DTOs.Chat;

namespace Sports.Application.Interfaces;

public interface IChatService
{
    Task<IEnumerable<ChatMessageDto>> GetGameMessagesAsync(int gameId, string currentUserId);
    Task<ChatMessageDto> SendMessageAsync(int gameId, string userId, SendMessageDto dto);
    Task<bool> DeleteMessageAsync(int messageId, string userId);
}