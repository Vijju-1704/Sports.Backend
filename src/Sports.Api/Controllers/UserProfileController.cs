using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sports.Application.Interfaces;
using Sports.Application.DTOs.UserProfile;

namespace Sports.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService UserProfileService;

    public UserProfileController(IUserProfileService userProfileService)
    {
        UserProfileService = userProfileService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetMyProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var profile = await UserProfileService.GetUserProfileAsync(userId);
        if (profile == null) return NotFound();

        return Ok(profile);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserProfileDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await UserProfileService.UpdateProfileAsync(userId, dto);
        if (!result) return BadRequest(new { message = "Failed to update profile" });

        return Ok(new { message = "Profile updated successfully" });
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserProfileDto>> GetUserProfile(string userId)
    {
        var profile = await UserProfileService.GetUserProfileAsync(userId);
        if (profile == null) return NotFound();

        return Ok(profile);
    }

    // ========== SPORT PROFILES ==========

    [HttpGet("me/sports")]
    public async Task<ActionResult<IEnumerable<UserSportProfileDto>>> GetMySportProfiles()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var profiles = await UserProfileService.GetUserSportProfilesAsync(userId);
        return Ok(profiles);
    }

    [HttpPost("me/sports")]
    public async Task<ActionResult<UserSportProfileDto>> AddSportProfile([FromBody] CreateUserSportProfileDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var profile = await UserProfileService.AddSportProfileAsync(userId, dto);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("me/sports/{profileId}")]
    public async Task<IActionResult> UpdateSportProfile(int profileId, [FromBody] CreateUserSportProfileDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await UserProfileService.UpdateSportProfileAsync(userId, profileId, dto);
        if (!result) return NotFound();

        return Ok(new { message = "Sport profile updated" });
    }

    [HttpDelete("me/sports/{profileId}")]
    public async Task<IActionResult> RemoveSportProfile(int profileId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await UserProfileService.RemoveSportProfileAsync(userId, profileId);
        if (!result) return NotFound();

        return Ok(new { message = "Sport profile removed" });
    }
}