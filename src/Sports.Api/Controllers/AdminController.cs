using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sports.Application.DTOs;
using Sports.Application.DTOs.Admin;
using Sports.Application.DTOs.Games;
using Sports.Application.Interfaces;

namespace Sports.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]  // Backward compatibility
[ApiController]
[ApiVersion("1.0")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAdminService AdminService;

    public AdminController(IAdminService adminService)
    {
        AdminService = adminService;
    }

    // ========== STATISTICS ==========

    /// <summary>
    /// Get admin dashboard statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(AdminStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminStatsDto>> GetStatistics()
    {
        var stats = await AdminService.GetStatisticsAsync();
        return Ok(stats);
    }

    // ========== SPORTS ==========

    /// <summary>
    /// Get all sports with optional pagination
    /// </summary>
    [HttpGet("sports")]
    [ProducesResponseType(typeof(IEnumerable<SportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<SportDto>>> GetAllSports(
        [FromQuery] int? pageNumber = null,
        [FromQuery] int? pageSize = null)
    {
        var sports = await AdminService.GetAllSportsAsync();
        
        // If pagination params provided, apply pagination
        if (pageNumber.HasValue && pageSize.HasValue)
        {
            var pagedSports = sports
                .Skip((pageNumber.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
            
            return Ok(new
            {
                Items = pagedSports,
                TotalCount = sports.Count(),
                PageNumber = pageNumber.Value,
                PageSize = pageSize.Value,
                TotalPages = (int)Math.Ceiling(sports.Count() / (double)pageSize.Value),
                HasPrevious = pageNumber.Value > 1,
                HasNext = pageNumber.Value < (int)Math.Ceiling(sports.Count() / (double)pageSize.Value)
            });
        }
        
        return Ok(sports);
    }

    /// <summary>
    /// Get a specific sport by ID
    /// </summary>
    [HttpGet("sports/{id}")]
    [ProducesResponseType(typeof(SportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SportDto>> GetSport(int id)
    {
        var sport = await AdminService.GetSportByIdAsync(id);
        if (sport == null) return NotFound();
        return Ok(sport);
    }

    /// <summary>
    /// Create a new sport
    /// </summary>
    [HttpPost("sports")]
    [ProducesResponseType(typeof(SportDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SportDto>> CreateSport([FromBody] CreateSportDto dto)
    {
        var sport = await AdminService.CreateSportAsync(dto);
        return CreatedAtAction(nameof(GetSport), new { id = sport.SportId }, sport);
    }

    /// <summary>
    /// Update an existing sport
    /// </summary>
    [HttpPut("sports/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateSport(int id, [FromBody] CreateSportDto dto)
    {
        var result = await AdminService.UpdateSportAsync(id, dto);
        if (!result) return NotFound();
        return Ok(new { message = "Sport updated successfully" });
    }

    /// <summary>
    /// Delete a sport
    /// </summary>
    [HttpDelete("sports/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

    /// <summary>
    /// Get all venues with optional pagination
    /// </summary>
    [HttpGet("venues")]
    [ProducesResponseType(typeof(IEnumerable<VenueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<VenueDto>>> GetAllVenues(
        [FromQuery] int? pageNumber = null,
        [FromQuery] int? pageSize = null)
    {
        var venues = await AdminService.GetAllVenuesAsync();
        
        if (pageNumber.HasValue && pageSize.HasValue)
        {
            var pagedVenues = venues
                .Skip((pageNumber.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
            
            return Ok(new
            {
                Items = pagedVenues,
                TotalCount = venues.Count(),
                PageNumber = pageNumber.Value,
                PageSize = pageSize.Value,
                TotalPages = (int)Math.Ceiling(venues.Count() / (double)pageSize.Value),
                HasPrevious = pageNumber.Value > 1,
                HasNext = pageNumber.Value < (int)Math.Ceiling(venues.Count() / (double)pageSize.Value)
            });
        }
        
        return Ok(venues);
    }

    /// <summary>
    /// Get a specific venue by ID
    /// </summary>
    [HttpGet("venues/{id}")]
    [ProducesResponseType(typeof(VenueDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<VenueDto>> GetVenue(int id)
    {
        var venue = await AdminService.GetVenueByIdAsync(id);
        if (venue == null) return NotFound();
        return Ok(venue);
    }

    /// <summary>
    /// Create a new venue
    /// </summary>
    [HttpPost("venues")]
    [ProducesResponseType(typeof(VenueDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<VenueDto>> CreateVenue([FromBody] CreateVenueDto dto)
    {
        var venue = await AdminService.CreateVenueAsync(dto);
        return CreatedAtAction(nameof(GetVenue), new { id = venue.VenueId }, venue);
    }

    /// <summary>
    /// Update an existing venue
    /// </summary>
    [HttpPut("venues/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateVenue(int id, [FromBody] CreateVenueDto dto)
    {
        var result = await AdminService.UpdateVenueAsync(id, dto);
        if (!result) return NotFound();
        return Ok(new { message = "Venue updated successfully" });
    }

    /// <summary>
    /// Delete a venue
    /// </summary>
    [HttpDelete("venues/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

    /// <summary>
    /// Get all employees with optional pagination
    /// </summary>
    [HttpGet("employees")]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAllEmployees(
        [FromQuery] int? pageNumber = null,
        [FromQuery] int? pageSize = null)
    {
        var employees = await AdminService.GetAllEmployeesAsync();
        
        if (pageNumber.HasValue && pageSize.HasValue)
        {
            var pagedEmployees = employees
                .Skip((pageNumber.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
            
            return Ok(new
            {
                Items = pagedEmployees,
                TotalCount = employees.Count(),
                PageNumber = pageNumber.Value,
                PageSize = pageSize.Value,
                TotalPages = (int)Math.Ceiling(employees.Count() / (double)pageSize.Value),
                HasPrevious = pageNumber.Value > 1,
                HasNext = pageNumber.Value < (int)Math.Ceiling(employees.Count() / (double)pageSize.Value)
            });
        }
        
        return Ok(employees);
    }

    /// <summary>
    /// Get a specific employee by ID
    /// </summary>
    [HttpGet("employees/{id}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
    {
        var employee = await AdminService.GetEmployeeByIdAsync(id);
        if (employee == null) return NotFound();
        return Ok(employee);
    }

    /// <summary>
    /// Create a new employee
    /// </summary>
    [HttpPost("employees")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

    /// <summary>
    /// Update an existing employee
    /// </summary>
    [HttpPut("employees/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
    {
        var result = await AdminService.UpdateEmployeeAsync(id, dto);
        if (!result) return NotFound();
        return Ok(new { message = "Employee updated successfully" });
    }

    /// <summary>
    /// Deactivate an employee
    /// </summary>
    [HttpPost("employees/{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeactivateEmployee(int id)
    {
        var result = await AdminService.DeactivateEmployeeAsync(id);
        if (!result) return NotFound();
        return Ok(new { message = "Employee deactivated" });
    }

    /// <summary>
    /// Activate an employee
    /// </summary>
    [HttpPost("employees/{id}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateEmployee(int id)
    {
        var result = await AdminService.ActivateEmployeeAsync(id);
        if (!result) return NotFound();
        return Ok(new { message = "Employee activated" });
    }

    // ========== USERS ==========

    /// <summary>
    /// Get all users with optional pagination
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers(
        [FromQuery] int? pageNumber = null,
        [FromQuery] int? pageSize = null)
    {
        var users = await AdminService.GetAllUsersAsync();
        
        if (pageNumber.HasValue && pageSize.HasValue)
        {
            var pagedUsers = users
                .Skip((pageNumber.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
            
            return Ok(new
            {
                Items = pagedUsers,
                TotalCount = users.Count(),
                PageNumber = pageNumber.Value,
                PageSize = pageSize.Value,
                TotalPages = (int)Math.Ceiling(users.Count() / (double)pageSize.Value),
                HasPrevious = pageNumber.Value > 1,
                HasNext = pageNumber.Value < (int)Math.Ceiling(users.Count() / (double)pageSize.Value)
            });
        }
        
        return Ok(users);
    }

    /// <summary>
    /// Get a specific user by ID
    /// </summary>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserDto>> GetUser(string userId)
    {
        var user = await AdminService.GetUserByIdAsync(userId);
        if (user == null) return NotFound();
        return Ok(user);
    }

    /// <summary>
    /// Deactivate a user
    /// </summary>
    [HttpPost("users/{userId}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeactivateUser(string userId)
    {
        var result = await AdminService.DeactivateUserAsync(userId);
        if (!result) return NotFound();
        return Ok(new { message = "User deactivated" });
    }

    /// <summary>
    /// Activate a user
    /// </summary>
    [HttpPost("users/{userId}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateUser(string userId)
    {
        var result = await AdminService.ActivateUserAsync(userId);
        if (!result) return NotFound();
        return Ok(new { message = "User activated" });
    }

    /// <summary>
    /// Change a user's role
    /// </summary>
    [HttpPost("users/{userId}/role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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