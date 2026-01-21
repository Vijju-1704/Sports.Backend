using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Sports.Application.DTOs.Games;
using Sports.Infrastructure.Services;

namespace Sports.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class JoinRequestsController : ControllerBase
{
    private readonly IJoinRequestService JoinRequestService;

    public JoinRequestsController(IJoinRequestService joinRequestService)
    {
        JoinRequestService = joinRequestService;
    }

    [HttpPost("game/{gameId}")]
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

    [HttpGet("game/{gameId}")]
    public async Task<ActionResult<IEnumerable<JoinRequestDto>>> GetGameJoinRequests(int gameId)
    {
        var requests = await JoinRequestService.GetGameJoinRequestsAsync(gameId);
        return Ok(requests);
    }

    [HttpGet("my-requests")]
    public async Task<ActionResult<IEnumerable<JoinRequestDto>>> GetMyJoinRequests()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var requests = await JoinRequestService.GetUserJoinRequestsAsync(userId);
        return Ok(requests);
    }

    [HttpPost("{requestId}/respond")]
    public async Task<IActionResult> RespondToJoinRequest(int requestId, [FromBody] RespondToJoinRequestDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await JoinRequestService.RespondToJoinRequestAsync(requestId, userId, dto);
        if (!result) return BadRequest(new { message = "Failed to respond to request" });

        return Ok(new { message = dto.Approve ? "Request approved" : "Request rejected" });
    }

    [HttpDelete("{requestId}")]
    public async Task<IActionResult> CancelJoinRequest(int requestId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await JoinRequestService.CancelJoinRequestAsync(requestId, userId);
        if (!result) return BadRequest(new { message = "Failed to cancel request" });

        return Ok(new { message = "Request cancelled" });
    }
}