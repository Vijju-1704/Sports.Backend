using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sports.Domain.Entities;

public class ChatMessage
{
    [Key]
    public int MessageId { get; set; }
    
    public int GameId { get; set; }
    [ForeignKey("GameId")]
    public Game Game { get; set; } = null!;
    
    public string SenderUserId { get; set; } = string.Empty; // FK to AspNetUser
    
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
