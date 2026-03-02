namespace Sports.Application.DTOs.Ratings;

/// DTO for the rate players page - contains game info and players to rate
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

/// DTO for individual player that can be rated
public class PlayerToRateDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public bool IsHost { get; set; }
    public string? CurrentSkillLevel { get; set; }
}

/// DTO for submitting ratings for multiple players
public class SubmitRatingsDto
{
    public int GameId { get; set; }
    public List<PlayerRatingDto> Ratings { get; set; } = new();
}

/// DTO for a single player rating
public class PlayerRatingDto
{
    public string UserId { get; set; } = string.Empty;
    public int SkillRating { get; set; } // 1-5 stars
    public bool WasOnTime { get; set; }
    public string? Comments { get; set; }
}

/// DTO for viewing a rating result
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

/// DTO for viewing all ratings for a game
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

/// DTO for player's rating summary within a game
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
