using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Sports.Application.DTOs.Games;
using Sports.Application.Interfaces;

namespace Sports.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]  
[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
public class VenuesController : ControllerBase
{
    private readonly IAdminService AdminService;

    public VenuesController(IAdminService adminService)
    {
        AdminService = adminService;
    }

    /// <summary>
    /// Get all venues (public endpoint)
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VenueDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VenueDto>>> GetAll()
    {
        var venues = await AdminService.GetAllVenuesAsync();
        return Ok(venues);
    }
}