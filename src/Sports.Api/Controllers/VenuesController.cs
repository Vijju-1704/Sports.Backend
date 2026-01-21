using Microsoft.AspNetCore.Mvc;
using Sports.Application.DTOs.Games;
using Sports.Application.Interfaces;

[Route("api/[controller]")]
[ApiController]
public class VenuesController : ControllerBase
{
    private readonly IAdminService AdminService;

    public VenuesController(IAdminService adminService)
    {
        AdminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VenueDto>>> GetAll()
    {
        var venues = await AdminService.GetAllVenuesAsync();
        return Ok(venues);
    }
}