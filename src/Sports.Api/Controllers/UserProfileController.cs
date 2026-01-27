using Asp.Versioning;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sports.Application.Interfaces;
using Sports.Application.DTOs.UserProfile;

namespace Sports.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]  // Backward compatibility
[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Produces("application/json")]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService UserProfileService;

    public UserProfileController(IUserProfileService userProfileService)
    {
        UserProfileService = userProfileService;
    }

    /// <summary>
    /// Get the current user's profile
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileDto>> GetMyProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var profile = await UserProfileService.GetUserProfileAsync(userId);
        if (profile == null) return NotFound();

        return Ok(profile);
    }

    /// <summary>
    /// Update the current user's profile
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserProfileDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await UserProfileService.UpdateProfileAsync(userId, dto);
        if (!result) return BadRequest(new { message = "Failed to update profile" });

        return Ok(new { message = "Profile updated successfully" });
    }

    /// <summary>
    /// Get a user's profile by ID
    /// </summary>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileDto>> GetUserProfile(string userId)
    {
        var profile = await UserProfileService.GetUserProfileAsync(userId);
        if (profile == null) return NotFound();

        return Ok(profile);
    }

    // ========== SPORT PROFILES ==========

    /// <summary>
    /// Get the current user's sport profiles
    /// </summary>
    [HttpGet("me/sports")]
    [ProducesResponseType(typeof(IEnumerable<UserSportProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<UserSportProfileDto>>> GetMySportProfiles()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var profiles = await UserProfileService.GetUserSportProfilesAsync(userId);
        return Ok(profiles);
    }

    /// <summary>
    /// Add a sport profile for the current user
    /// </summary>
    [HttpPost("me/sports")]
    [ProducesResponseType(typeof(UserSportProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Update a sport profile
    /// </summary>
    [HttpPut("me/sports/{profileId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateSportProfile(int profileId, [FromBody] CreateUserSportProfileDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await UserProfileService.UpdateSportProfileAsync(userId, profileId, dto);
        if (!result) return NotFound();

        return Ok(new { message = "Sport profile updated" });
    }

    /// <summary>
    /// Remove a sport profile
    /// </summary>
    [HttpDelete("me/sports/{profileId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveSportProfile(int profileId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await UserProfileService.RemoveSportProfileAsync(userId, profileId);
        if (!result) return NotFound();

        return Ok(new { message = "Sport profile removed" });
    }
}