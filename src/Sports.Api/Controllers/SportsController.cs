using Microsoft.AspNetCore.Mvc;
using Sports.Application.DTOs.Games;
using Sports.Application.Interfaces;

[Route("api/[controller]")]
[ApiController]
public class SportsController : ControllerBase
{
    private readonly IAdminService _adminService;

    public SportsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SportDto>>> GetAll()
    {
        var sports = await _adminService.GetAllSportsAsync();
        return Ok(sports);
    }
}