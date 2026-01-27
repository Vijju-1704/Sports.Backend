using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sports.Domain.Entities;

public enum FriendshipStatus
{
    Pending,
    Accepted,
    Blocked,
    Declined
}

public class Friendship
{
    [Key]
    public int FriendshipId { get; set; }

    public string RequesterId { get; set; } = string.Empty;
    
    public string AddresseeId { get; set; } = string.Empty;

    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ActionedAt { get; set; }
}
