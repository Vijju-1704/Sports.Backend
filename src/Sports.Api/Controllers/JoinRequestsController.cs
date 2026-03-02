using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Sports.Application.DTOs.Games;
using Sports.Infrastructure.Services;

namespace Sports.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")] 
[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Produces("application/json")]
public class JoinRequestsController : ControllerBase
{
    private readonly IJoinRequestService JoinRequestService;

    public JoinRequestsController(IJoinRequestService joinRequestService)
    {
        JoinRequestService = joinRequestService;
    }

    /// <summary>
    /// Create a join request for a game
    /// </summary>
    /// <param name="gameId"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("game/{gameId}")]
    [ProducesResponseType(typeof(JoinRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<JoinRequestDto>> CreateJoinRequest(int gameId, [FromBody] CreateJoinRequestDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var request = await JoinRequestService.CreateJoinRequestAsync(gameId, userId, dto);
            return Ok(request);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    ///     Get all join requests for a game
    /// </summary>
    /// <param name="gameId"></param>
    /// <returns></returns>
    [HttpGet("game/{gameId}")]
    [ProducesResponseType(typeof(IEnumerable<JoinRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<JoinRequestDto>>> GetGameJoinRequests(int gameId)
    {
        var requests = await JoinRequestService.GetGameJoinRequestsAsync(gameId);
        return Ok(requests);
    }

    /// <summary>
    /// Get current user's join requests
    /// </summary>
    /// <returns></returns>
    [HttpGet("my-requests")]
    [ProducesResponseType(typeof(IEnumerable<JoinRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<JoinRequestDto>>> GetMyJoinRequests()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var requests = await JoinRequestService.GetUserJoinRequestsAsync(userId);
        return Ok(requests);
    }

    /// <summary>
    /// Respond to a join request (approve or reject)
    /// </summary>
    /// <param name="requestId"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("{requestId}/respond")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RespondToJoinRequest(int requestId, [FromBody] RespondToJoinRequestDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await JoinRequestService.RespondToJoinRequestAsync(requestId, userId, dto);
        if (!result) return BadRequest(new { message = "Failed to respond to request" });

        return Ok(new { message = dto.Approve ? "Request approved" : "Request rejected" });
    }

    /// <summary>
    /// Cancel a join request
    /// </summary>
    /// <param name="requestId"></param>
    /// <returns></returns>
    [HttpDelete("{requestId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelJoinRequest(int requestId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await JoinRequestService.CancelJoinRequestAsync(requestId, userId);
        if (!result) return BadRequest(new { message = "Failed to cancel request" });

        return Ok(new { message = "Request cancelled" });
    }
}