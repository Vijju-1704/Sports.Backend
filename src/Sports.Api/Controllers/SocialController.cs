using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sports.Application.Interfaces;
using System.Security.Claims;

namespace Sports.Api.Controllers;

[Authorize]
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class SocialController : ControllerBase
{
    private readonly IFriendService _friendService;

    public SocialController(IFriendService friendService)
    {
        _friendService = friendService;
    }

    [HttpPost("friend/request")]
    public async Task<IActionResult> SendFriendRequest([FromQuery] string addresseeId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _friendService.SendFriendRequestAsync(userId, addresseeId);
        
        if (!result) return BadRequest("Unable to send request. It may already exist.");
        
        return Ok("Friend request sent.");
    }

    [HttpPost("friend/accept")]
    public async Task<IActionResult> AcceptFriendRequest([FromQuery] int friendshipId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _friendService.AcceptFriendRequestAsync(friendshipId, userId);

        if (!result) return BadRequest("Unable to accept request.");

        return Ok("Friend request accepted.");
    }

    [HttpGet("friends")]
    public async Task<IActionResult> GetFriends()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var friends = await _friendService.GetFriendsAsync(userId);
        
        // Map to DTO if needed to hide IDs or format
        // For now returning raw entities as per V1 style (or simple DTO)
        return Ok(friends);
    }
    
    [HttpGet("friends/pending")]
    public async Task<IActionResult> GetPending()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var pending = await _friendService.GetPendingRequestsAsync(userId);
        return Ok(pending);
    }

    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var feed = await _friendService.GetActivityFeedAsync(userId);
        return Ok(feed);
    }

    [HttpGet("users/search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query, [FromServices] IUserProfileService userProfileService)
    {
        var users = await userProfileService.SearchUsersAsync(query);
        return Ok(users);
    }
}

