using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sports.Domain.Entities;

public enum ActivityType
{
    JoinedGame,
    CreatedGame,
    WonMatch,
    AchievementUnlocked,
    FriendRequestAccepted
}

public class ActivityFeed
{
    [Key]
    public int ActivityId { get; set; }

    public string ActorId { get; set; } = string.Empty; // User who did it

    public ActivityType Type { get; set; }

    public string TargetId { get; set; } = string.Empty; // GameID, FriendID, etc.

    public string Description { get; set; } = string.Empty;

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
