using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Sports.Application.DTOs.Games;
using Sports.Application.Interfaces;

namespace Sports.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]  // Backward compatibility
[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
public class SportsController : ControllerBase
{
    private readonly IAdminService AdminService;

    public SportsController(IAdminService adminService)
    {
        AdminService = adminService;
    }

    /// <summary>
    /// Get all sports (public endpoint)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SportDto>>> GetAll()
    {
        var sports = await AdminService.GetAllSportsAsync();
        return Ok(sports);
    }
}