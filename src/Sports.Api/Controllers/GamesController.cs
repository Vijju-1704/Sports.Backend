using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sports.Application.DTOs.Games;
using Sports.Application.Interfaces;
using Sports.Domain.Constants;
using Sports.Domain.Enums;
using System.Security.Claims;

namespace Sports.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]  
[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Produces("application/json")]
public class GamesController : ControllerBase
{
    private readonly IGameService GameService;

    public GamesController(IGameService gameService)
    {
        GameService = gameService;
    }

    /// <summary>
    ///     Get all active games with optional filtering
    /// </summary>
    /// <param name="search"></param>
    /// <param name="date"></param>
    /// <param name="sportId"></param>
    /// <param name="city"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GameDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<GameDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] DateTime? date,
        [FromQuery] int? sportId,
        [FromQuery] string? city)
    {
        var games = await GameService.GetAllGamesAsync(search, date, sportId, city);
        return Ok(games);
    }

    /// <summary>
    ///     Get ALL games including past (completed/cancelled) for admin
    /// </summary>
    /// <returns></returns>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<GameDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<GameDto>>> GetAllIncludingPast()
    {
        var games = await GameService.GetAllGamesIncludingPastAsync();
        return Ok(games);
    }

    /// <summary>
    ///     Get games with pagination
    /// </summary>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <param name="search"></param>
    /// <param name="sportId"></param>
    /// <param name="city"></param>
    /// <returns></returns>
    [HttpGet("paginated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> GetAllPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? search = null,
        [FromQuery] int? sportId = null,
        [FromQuery] string? city = null)
    {
        var result = await GameService.GetGamesPaginatedAsync(page, pageSize, search, sportId, city);
        return Ok(result);
    }

    /// <summary>
    ///     Get a specific game by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GameDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GameDetailDto>> GetById(int id)
    {
        var game = await GameService.GetGameByIdAsync(id);
        if (game == null) return NotFound();
        return Ok(game);
    }

    /// <summary>
    ///     Create a new game
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(GameDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GameDto>> Create([FromBody] CreateGameDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var createdGame = await GameService.CreateGameAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = createdGame.GameId }, createdGame);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    ///     Update an existing game
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] CreateGameDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var success = await GameService.UpdateGameAsync(id, dto, userId);
        if (!success) return BadRequest(MessageStrings.UnableToUpdateGame);

        return Ok(new { message = "Game updated" });
    }

    /// <summary>
    ///     Join a game
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/join")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Join(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await GameService.JoinGameAsync(id, userId);
        if (!result) return BadRequest(MessageStrings.UnableToJoinGame);

        return Ok(new { message = MessageStrings.JoinedSuccessfully });
    }

    /// <summary>
    ///     Leave a game
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/leave")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Leave(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await GameService.LeaveGameAsync(id, userId);
        if (!result) return BadRequest(MessageStrings.UnableToLeaveGame);

        return Ok(new { message = MessageStrings.LeftGameSuccessfully });
    }

    /// <summary>
    ///     Cancel a game (host only)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await GameService.CancelGameAsync(id, userId);
        if (!result) return BadRequest(MessageStrings.UnableToCancelGame);

        return Ok(new { message = "Game cancelled" });
    }

    /// <summary>
    ///     Update game status
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateGameStatus(int id, [FromBody] UpdateGameStatusDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var status = Enum.Parse<GameStatus>(dto.Status);
        var result = await GameService.UpdateGameStatusAsync(id, userId, status);
        if (!result) return BadRequest(MessageStrings.UnableToUpdateGameStatus);

        return Ok(new { message = MessageStrings.GameStatusUpdated });
    }
}