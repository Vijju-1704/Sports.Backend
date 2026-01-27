using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Sports.Application.DTOs.Auth;
using Sports.Application.Interfaces;

namespace Sports.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]  // Backward compatibility
[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService AuthService;

    public AuthController(IAuthService authService)
    {
        AuthService = authService;
    }

    /// <summary>
    /// Authenticate user and get JWT token
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var response = await AuthService.LoginAsync(loginDto);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
         try
        {
            var response = await AuthService.RegisterAsync(registerDto);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
