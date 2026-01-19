using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sports.Application.DTOs.Games;
using Sports.Application.Interfaces;
using Sports.Domain.Enums;
using System.Security.Claims;

namespace Sports.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] DateTime? date,
        [FromQuery] int? sportId,
        [FromQuery] string? city)
    {
        var games = await _gameService.GetAllGamesAsync(search, date, sportId, city);
        return Ok(games);
    }

    /// <summary>
    /// Get games with pagination
    /// </summary>
    [HttpGet("paginated")]
    public async Task<ActionResult> GetAllPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? search = null,
        [FromQuery] int? sportId = null,
        [FromQuery] string? city = null)
    {
        var result = await _gameService.GetGamesPaginatedAsync(page, pageSize, search, sportId, city);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GameDetailDto>> GetById(int id)
    {
        var game = await _gameService.GetGameByIdAsync(id);
        if (game == null) return NotFound();
        return Ok(game);
    }

    [HttpPost]
    public async Task<ActionResult<GameDto>> Create([FromBody] CreateGameDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var createdGame = await _gameService.CreateGameAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = createdGame.GameId }, createdGame);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateGameDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var success = await _gameService.UpdateGameAsync(id, dto, userId);
        if (!success) return BadRequest("Unable to update game (not found or not host)");

        return Ok(new { message = "Game updated" });
    }

    [HttpPost("{id}/join")]
    public async Task<IActionResult> Join(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _gameService.JoinGameAsync(id, userId);
        if (!result) return BadRequest("Unable to join game (Full, Closed, or Already Joined)");

        return Ok(new { message = "Joined successfully" });
    }

    // ✅ NEW: Leave Game Feature
    [HttpPost("{id}/leave")]
    public async Task<IActionResult> Leave(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _gameService.LeaveGameAsync(id, userId);
        if (!result) return BadRequest("Unable to leave game (Not a participant or you are the host)");

        return Ok(new { message = "Left game successfully" });
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _gameService.CancelGameAsync(id, userId);
        if (!result) return BadRequest("Unable to cancel game (Not Authorized or Not Found)");

        return Ok(new { message = "Game cancelled" });
    }

    [HttpPost("{id}/status")]
    public async Task<IActionResult> UpdateGameStatus(int id, [FromBody] UpdateGameStatusDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var status = Enum.Parse<GameStatus>(dto.Status);
        var result = await _gameService.UpdateGameStatusAsync(id, userId, status);
        if (!result) return BadRequest("Unable to update game status");

        return Ok(new { message = "Game status updated" });
    }
}