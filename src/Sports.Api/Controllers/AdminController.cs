using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sports.Application.DTOs;
using Sports.Application.DTOs.Admin;
using Sports.Application.DTOs.Games;
using Sports.Application.Interfaces;

namespace Sports.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService AdminService;

    public AdminController(IAdminService adminService)
    {
        AdminService = adminService;
    }

    // ========== STATISTICS ==========

    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsDto>> GetStatistics()
    {
        var stats = await AdminService.GetStatisticsAsync();
        return Ok(stats);
    }

    // ========== SPORTS ==========

    [HttpGet("sports")]
    public async Task<ActionResult<IEnumerable<SportDto>>> GetAllSports()
    {
        var sports = await AdminService.GetAllSportsAsync();
        return Ok(sports);
    }

    [HttpGet("sports/{id}")]
    public async Task<ActionResult<SportDto>> GetSport(int id)
    {
        var sport = await AdminService.GetSportByIdAsync(id);
        if (sport == null) return NotFound();
        return Ok(sport);
    }

    [HttpPost("sports")]
    public async Task<ActionResult<SportDto>> CreateSport([FromBody] CreateSportDto dto)
    {
        var sport = await AdminService.CreateSportAsync(dto);
        return CreatedAtAction(nameof(GetSport), new { id = sport.SportId }, sport);
    }

    [HttpPut("sports/{id}")]
    public async Task<IActionResult> UpdateSport(int id, [FromBody] CreateSportDto dto)
    {
        var result = await AdminService.UpdateSportAsync(id, dto);
        if (!result) return NotFound();
        return Ok(new { message = "Sport updated successfully" });
    }

    [HttpDelete("sports/{id}")]
    public async Task<IActionResult> DeleteSport(int id)
    {
        try
        {
            var result = await AdminService.DeleteSportAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Sport deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ========== VENUES ==========

    [HttpGet("venues")]
    public async Task<ActionResult<IEnumerable<VenueDto>>> GetAllVenues()
    {
        var venues = await AdminService.GetAllVenuesAsync();
        return Ok(venues);
    }

    [HttpGet("venues/{id}")]
    public async Task<ActionResult<VenueDto>> GetVenue(int id)
    {
        var venue = await AdminService.GetVenueByIdAsync(id);
        if (venue == null) return NotFound();
        return Ok(venue);
    }

    [HttpPost("venues")]
    public async Task<ActionResult<VenueDto>> CreateVenue([FromBody] CreateVenueDto dto)
    {
        var venue = await AdminService.CreateVenueAsync(dto);
        return CreatedAtAction(nameof(GetVenue), new { id = venue.VenueId }, venue);
    }

    [HttpPut("venues/{id}")]
    public async Task<IActionResult> UpdateVenue(int id, [FromBody] CreateVenueDto dto)
    {
        var result = await AdminService.UpdateVenueAsync(id, dto);
        if (!result) return NotFound();
        return Ok(new { message = "Venue updated successfully" });
    }

    [HttpDelete("venues/{id}")]
    public async Task<IActionResult> DeleteVenue(int id)
    {
        try
        {
            var result = await AdminService.DeleteVenueAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Venue deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ========== EMPLOYEES ==========

    [HttpGet("employees")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAllEmployees()
    {
        var employees = await AdminService.GetAllEmployeesAsync();
        return Ok(employees);
    }

    [HttpGet("employees/{id}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
    {
        var employee = await AdminService.GetEmployeeByIdAsync(id);
        if (employee == null) return NotFound();
        return Ok(employee);
    }

    [HttpPost("employees")]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto dto)
    {
        try
        {
            await AdminService.CreateEmployeeAsync(dto);
            return Ok(new { message = "Employee created successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("employees/{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
    {
        var result = await AdminService.UpdateEmployeeAsync(id, dto);
        if (!result) return NotFound();
        return Ok(new { message = "Employee updated successfully" });
    }

    [HttpPost("employees/{id}/deactivate")]
    public async Task<IActionResult> DeactivateEmployee(int id)
    {
        var result = await AdminService.DeactivateEmployeeAsync(id);
        if (!result) return NotFound();
        return Ok(new { message = "Employee deactivated" });
    }

    [HttpPost("employees/{id}/activate")]
    public async Task<IActionResult> ActivateEmployee(int id)
    {
        var result = await AdminService.ActivateEmployeeAsync(id);
        if (!result) return NotFound();
        return Ok(new { message = "Employee activated" });
    }

    // ========== USERS ==========

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        var users = await AdminService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("users/{userId}")]
    public async Task<ActionResult<UserDto>> GetUser(string userId)
    {
        var user = await AdminService.GetUserByIdAsync(userId);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost("users/{userId}/deactivate")]
    public async Task<IActionResult> DeactivateUser(string userId)
    {
        var result = await AdminService.DeactivateUserAsync(userId);
        if (!result) return NotFound();
        return Ok(new { message = "User deactivated" });
    }

    [HttpPost("users/{userId}/activate")]
    public async Task<IActionResult> ActivateUser(string userId)
    {
        var result = await AdminService.ActivateUserAsync(userId);
        if (!result) return NotFound();
        return Ok(new { message = "User activated" });
    }

    [HttpPost("users/{userId}/role")]
    public async Task<IActionResult> ChangeUserRole(string userId, [FromBody] ChangeRoleDto dto)
    {
        try
        {
            var result = await AdminService.ChangeUserRoleAsync(userId, dto.Role);
            if (!result) return NotFound();
            return Ok(new { message = $"User role changed to {dto.Role}" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}