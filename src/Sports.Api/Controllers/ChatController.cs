using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sports.Application.DTOs.Chat;
using Sports.Application.Interfaces;
using System.Security.Claims;

namespace Sports.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]  // Backward compatibility
[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Produces("application/json")]
public class ChatController : ControllerBase
{
    private readonly IChatService ChatService;

    public ChatController(IChatService chatService)
    {
        ChatService = chatService;
    }

    /// <summary>
    /// Get chat messages for a game
    /// </summary>
    [HttpGet("game/{gameId}")]
    [ProducesResponseType(typeof(IEnumerable<ChatMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetGameMessages(int gameId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var messages = await ChatService.GetGameMessagesAsync(gameId, userId);
        return Ok(messages);
    }

    /// <summary>
    /// Send a chat message in a game
    /// </summary>
    [HttpPost("game/{gameId}")]
    [ProducesResponseType(typeof(ChatMessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ChatMessageDto>> SendMessage(int gameId, [FromBody] SendMessageDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var message = await ChatService.SendMessageAsync(gameId, userId, dto);
            return Ok(message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    /// <summary>
    /// Delete a chat message
    /// </summary>
    [HttpDelete("{messageId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteMessage(int messageId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await ChatService.DeleteMessageAsync(messageId, userId);
        if (!result) return BadRequest("Unable to delete message");

        return Ok(new { message = "Message deleted" });
    }
}