namespace Sports.Application.DTOs.Chat;

public class ChatMessageDto
{
    public int MessageId { get; set; }
    public int GameId { get; set; }
    public string SenderUserId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsCurrentUser { get; set; }
}

public class SendMessageDto
{
    public string Content { get; set; } = string.Empty;
}