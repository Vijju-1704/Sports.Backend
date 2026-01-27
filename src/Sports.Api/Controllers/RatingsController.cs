using Asp.Versioning;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sports.Application.DTOs.Ratings;
using Sports.Application.Interfaces;

namespace Sports.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]  // Backward compatibility
[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Produces("application/json")]
public class RatingsController : ControllerBase
{
    private readonly IRatingService RatingService;

    public RatingsController(IRatingService ratingService)
    {
        RatingService = ratingService;
    }

    /// <summary>
    /// Get players to rate for a completed game
    /// </summary>
    [HttpGet("game/{gameId}")]
    [ProducesResponseType(typeof(RatePlayersDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RatePlayersDto>> GetPlayersToRate(int gameId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await RatingService.GetPlayersToRateAsync(gameId, userId);
        if (result == null)
            return NotFound(new { message = "Game not found or you are not a participant" });

        return Ok(result);
    }

    /// <summary>
    /// Submit ratings for players in a completed game
    /// </summary>
    [HttpPost("game/{gameId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SubmitRatings(int gameId, [FromBody] SubmitRatingsDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        dto.GameId = gameId;
        var result = await RatingService.SubmitRatingsAsync(gameId, userId, dto);
        
        if (!result)
            return BadRequest(new { message = "Failed to submit ratings" });

        return Ok(new { message = "Ratings submitted successfully" });
    }

    /// <summary>
    /// Check if user has already rated for a game
    /// </summary>
    [HttpGet("game/{gameId}/hasrated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<bool>> HasUserRated(int gameId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var hasRated = await RatingService.HasUserRatedAsync(gameId, userId);
        return Ok(new { hasRated });
    }

    /// <summary>
    /// Get all ratings for a game (Admin/viewing)
    /// </summary>
    [HttpGet("game/{gameId}/results")]
    [ProducesResponseType(typeof(GameRatingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GameRatingsDto>> GetGameRatings(int gameId)
    {
        var result = await RatingService.GetGameRatingsAsync(gameId);
        if (result == null)
            return NotFound(new { message = "Game not found" });

        return Ok(result);
    }

    /// <summary>
    /// Send rating notifications for a completed game (Admin only)
    /// </summary>
    [HttpPost("game/{gameId}/notify")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SendRatingNotifications(int gameId)
    {
        await RatingService.SendRatingNotificationsAsync(gameId);
        return Ok(new { message = "Rating notifications sent" });
    }
}
