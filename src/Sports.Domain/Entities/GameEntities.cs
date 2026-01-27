using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sports.Domain.Enums;

namespace Sports.Domain.Entities;

public class Game
{
    [Key]
    public int GameId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [ForeignKey("Sport")]
    public int SportId { get; set; }
    public Sport Sport { get; set; } = null!;

    [ForeignKey("Venue")]
    public int VenueId { get; set; }
    public Venue Venue { get; set; } = null!;

    public string HostUserId { get; set; } = string.Empty;

    public DateTime DateTime { get; set; }
    public int DurationMinutes { get; set; } = 60;

    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? CostPerPerson { get; set; }

    public string? EquipmentNeeded { get; set; }
    public GameStatus Status { get; set; } = GameStatus.Open;

    public bool RequireApproval { get; set; } = false;

    public ICollection<GameParticipant> Participants { get; set; } = new List<GameParticipant>();

    public ICollection<JoinRequest> JoinRequests { get; set; } = new List<JoinRequest>();
}

public class GameParticipant
{
    [Key]
    public int ParticipantId { get; set; }
    
    public int GameId { get; set; }
    [ForeignKey("GameId")]
    public Game Game { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty; // FK to AspNetUser
    
    public JoinStatus Status { get; set; } = JoinStatus.Confirmed;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public class UserSportProfile
{
    [Key]
    public int ProfileId { get; set; }
    
    public string UserId { get; set; } = string.Empty; // FK
    
    public int SportId { get; set; }
    [ForeignKey("SportId")]
    public Sport Sport { get; set; } = null!;
    
    public SkillLevel SkillLevel { get; set; }
    public int ExperienceYears { get; set; }
    public string? PreferredPosition { get; set; }
}


public class JoinRequest
{
    [Key]
    public int RequestId { get; set; }

    public int GameId { get; set; }
    [ForeignKey("GameId")]
    public Game Game { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;

    public string? Message { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
    public string? ResponseMessage { get; set; }
}

public class Notification
{
    [Key]
    public int NotificationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? RelatedGameId { get; set; } // Link to game for rating notifications
}

public class UserEmployeeMap
{
    [Key]
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    [ForeignKey("EmployeeId")]
    public EmployeeDirectory Employee { get; set; } = null!;
}

public class PlayerRating
{
    [Key]
    public int PlayerRatingId { get; set; }
    
    public int GameId { get; set; }
    [ForeignKey("GameId")]
    public Game Game { get; set; } = null!;
    
    public string RaterId { get; set; } = string.Empty; // Who gave the rating
    public string RatedUserId { get; set; } = string.Empty; // Who received the rating
    
    public int SkillRating { get; set; } // 1-5 stars
    public bool WasOnTime { get; set; }
    public string? Comments { get; set; }
    
    public DateTime RatedAt { get; set; } = DateTime.UtcNow;
}
