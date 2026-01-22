using Sports.Application.DTOs.Ratings;

namespace Sports.Application.Interfaces;

public interface IRatingService
{
    /// <summary>
    /// Get players that a user can rate for a completed game
    /// </summary>
    Task<RatePlayersDto?> GetPlayersToRateAsync(int gameId, string raterId);
    
    /// <summary>
    /// Submit ratings for players in a game
    /// </summary>
    Task<bool> SubmitRatingsAsync(int gameId, string raterId, SubmitRatingsDto dto);
    
    /// <summary>
    /// Check if a user has already rated players in a game
    /// </summary>
    Task<bool> HasUserRatedAsync(int gameId, string userId);
    
    /// <summary>
    /// Get all ratings for a game (admin view)
    /// </summary>
    Task<GameRatingsDto?> GetGameRatingsAsync(int gameId);
    
    /// <summary>
    /// Send rating request notifications to all participants after game completion
    /// </summary>
    Task SendRatingNotificationsAsync(int gameId);
}
