using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sports.Application.DTOs.Chat;
using Sports.Application.Interfaces;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet("game/{gameId}")]
    public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetGameMessages(int gameId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var messages = await _chatService.GetGameMessagesAsync(gameId, userId);
        return Ok(messages);
    }

    [HttpPost("game/{gameId}")]
    public async Task<ActionResult<ChatMessageDto>> SendMessage(int gameId, [FromBody] SendMessageDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var message = await _chatService.SendMessageAsync(gameId, userId, dto);
            return Ok(message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessage(int messageId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _chatService.DeleteMessageAsync(messageId, userId);
        if (!result) return BadRequest("Unable to delete message");

        return Ok(new { message = "Message deleted" });
    }
}