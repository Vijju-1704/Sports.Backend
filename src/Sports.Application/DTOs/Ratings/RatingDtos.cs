namespace Sports.Application.DTOs.Ratings;

/// <summary>
/// DTO for the rate players page - contains game info and players to rate
/// </summary>
public class RatePlayersDto
{
    public int GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public string SportName { get; set; } = string.Empty;
    public string SportIcon { get; set; } = string.Empty;
    public DateTime GameDate { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public List<PlayerToRateDto> PlayersToRate { get; set; } = new();
}

/// <summary>
/// DTO for individual player that can be rated
/// </summary>
public class PlayerToRateDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public bool IsHost { get; set; }
    public string? CurrentSkillLevel { get; set; }
}

/// <summary>
/// DTO for submitting ratings for multiple players
/// </summary>
public class SubmitRatingsDto
{
    public int GameId { get; set; }
    public List<PlayerRatingDto> Ratings { get; set; } = new();
}

/// <summary>
/// DTO for a single player rating
/// </summary>
public class PlayerRatingDto
{
    public string UserId { get; set; } = string.Empty;
    public int SkillRating { get; set; } // 1-5 stars
    public bool WasOnTime { get; set; }
    public string? Comments { get; set; }
}

/// <summary>
/// DTO for viewing a rating result
/// </summary>
public class PlayerRatingResultDto
{
    public int RatingId { get; set; }
    public string RaterName { get; set; } = string.Empty;
    public string RatedPlayerName { get; set; } = string.Empty;
    public int SkillRating { get; set; }
    public bool WasOnTime { get; set; }
    public string? Comments { get; set; }
    public DateTime RatedAt { get; set; }
}

/// <summary>
/// DTO for viewing all ratings for a game
/// </summary>
public class GameRatingsDto
{
    public int GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public string SportName { get; set; } = string.Empty;
    public string SportIcon { get; set; } = string.Empty;
    public DateTime GameDate { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public int TotalParticipants { get; set; }
    public int TotalRatingsSubmitted { get; set; }
    public double AverageSkillRating { get; set; }
    public double OnTimePercentage { get; set; }
    public int PlayersRated { get; set; }
    public List<PlayerSummaryDto> PlayerSummaries { get; set; } = new();
    public List<PlayerRatingResultDto> AllRatings { get; set; } = new();
}

/// <summary>
/// DTO for player's rating summary within a game
/// </summary>
public class PlayerSummaryDto
{
    public string UserId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public bool IsHost { get; set; }
    public double AverageRating { get; set; }
    public double WasOnTimePercentage { get; set; }
    public int RatingsReceived { get; set; }
    public string? MostCommonComment { get; set; }
}
