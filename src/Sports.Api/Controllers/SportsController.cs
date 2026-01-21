using Microsoft.AspNetCore.Mvc;
using Sports.Application.DTOs.Games;
using Sports.Application.Interfaces;

[Route("api/[controller]")]
[ApiController]
public class SportsController : ControllerBase
{
    private readonly IAdminService AdminService;

    public SportsController(IAdminService adminService)
    {
        AdminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SportDto>>> GetAll()
    {
        var sports = await AdminService.GetAllSportsAsync();
        return Ok(sports);
    }
}