using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sports.Application.DTOs;
using Sports.Application.DTOs.Games;
using Sports.Application.Interfaces;
using Sports.Domain.Entities;
using System.Security.Claims;

namespace Sports.Api.Controllers;

// ========== ADMIN CONTROLLER ==========
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // Sports Endpoints
    [HttpGet("sports")]
    public async Task<ActionResult<IEnumerable<SportDto>>> GetAllSports()
    {
        var sports = await _adminService.GetAllSportsAsync();
        return Ok(sports);
    }

    [HttpGet("sports/{id}")]
    public async Task<ActionResult<SportDto>> GetSport(int id)
    {
        var sport = await _adminService.GetSportByIdAsync(id);
        if (sport == null) return NotFound();
        return Ok(sport);
    }

    [HttpPost("sports")]
    public async Task<ActionResult<SportDto>> CreateSport([FromBody] CreateSportDto dto)
    {
        var sport = await _adminService.CreateSportAsync(dto);
        return CreatedAtAction(nameof(GetSport), new { id = sport.SportId }, sport);
    }

    [HttpPut("sports/{id}")]
    public async Task<IActionResult> UpdateSport(int id, [FromBody] CreateSportDto dto)
    {
        var result = await _adminService.UpdateSportAsync(id, dto);
        if (!result) return NotFound();
        return Ok(new { message = "Sport updated successfully" });
    }

    [HttpDelete("sports/{id}")]
    public async Task<IActionResult> DeleteSport(int id)
    {
        var result = await _adminService.DeleteSportAsync(id);
        if (!result) return NotFound();
        return Ok(new { message = "Sport deleted successfully" });
    }

    // Venues Endpoints
    [HttpGet("venues")]
    public async Task<ActionResult<IEnumerable<VenueDto>>> GetAllVenues()
    {
        var venues = await _adminService.GetAllVenuesAsync();
        return Ok(venues);
    }

    [HttpGet("venues/{id}")]
    public async Task<ActionResult<VenueDto>> GetVenue(int id)
    {
        var venue = await _adminService.GetVenueByIdAsync(id);
        if (venue == null) return NotFound();
        return Ok(venue);
    }

    [HttpPost("venues")]
    public async Task<ActionResult<VenueDto>> CreateVenue([FromBody] CreateVenueDto dto)
    {
        var venue = await _adminService.CreateVenueAsync(dto);
        return CreatedAtAction(nameof(GetVenue), new { id = venue.VenueId }, venue);
    }

    [HttpPut("venues/{id}")]
    public async Task<IActionResult> UpdateVenue(int id, [FromBody] CreateVenueDto dto)
    {
        var result = await _adminService.UpdateVenueAsync(id, dto);
        if (!result) return NotFound();
        return Ok(new { message = "Venue updated successfully" });
    }

    [HttpDelete("venues/{id}")]
    public async Task<IActionResult> DeleteVenue(int id)
    {
        var result = await _adminService.DeleteVenueAsync(id);
        if (!result) return NotFound();
        return Ok(new { message = "Venue deleted successfully" });
    }

    // Employees Endpoints
    [HttpGet("employees")]
    public async Task<ActionResult<IEnumerable<EmployeeDirectory>>> GetAllEmployees()
    {
        var employees = await _adminService.GetAllEmployeesAsync();
        return Ok(employees);
    }

    [HttpGet("employees/{id}")]
    public async Task<ActionResult<EmployeeDirectory>> GetEmployee(int id)
    {
        var employee = await _adminService.GetEmployeeByIdAsync(id);
        if (employee == null) return NotFound();
        return Ok(employee);
    }

    [HttpPost("employees")]
    public async Task<ActionResult> CreateEmployee([FromBody] EmployeeDirectory employee)
    {
        await _adminService.CreateEmployeeAsync(employee);
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.EmployeeId }, employee);
    }

    [HttpPut("employees/{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeDirectory employee)
    {
        var result = await _adminService.UpdateEmployeeAsync(id, employee);
        if (!result) return NotFound();
        return Ok(new { message = "Employee updated successfully" });
    }

    [HttpDelete("employees/{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var result = await _adminService.DeleteEmployeeAsync(id);
        if (!result) return NotFound();
        return Ok(new { message = "Employee deleted successfully" });
    }
}