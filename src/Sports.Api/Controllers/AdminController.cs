using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sports.Application.DTOs.Admin;
using Sports.Application.DTOs.Games;
using Sports.Application.Interfaces;
using Sports.Domain.Constants;

namespace Sports.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]  
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
    /// <summary>
    /// Retrieves aggregated administrative statistics for the application.
    /// </summary>
    /// <remarks>Requires authentication and appropriate administrative permissions. Returns HTTP 200 with
    /// statistics data on success, 401 if the user is not authenticated, or 403 if the user lacks sufficient
    /// privileges.</remarks>
    /// <returns>An <see cref="ActionResult{T}"/> containing an <see cref="AdminStatsDto"/> with current statistics if the
    /// request is authorized; otherwise, an appropriate error response.</returns>
    // ========== STATISTICS ==========
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
    /// Retrieves a list of all available sports, with optional support for pagination.
    /// </summary>
    /// <remarks>When both <paramref name="pageNumber"/> and <paramref name="pageSize"/> are provided, the
    /// response includes pagination details such as total count, current page, page size, total pages, and navigation
    /// flags. If pagination parameters are omitted, the full list of sports is returned.</remarks>
    /// <param name="pageNumber">The page number to retrieve. If specified, must be greater than or equal to 1. If null, all sports are returned
    /// without pagination.</param>
    /// <param name="pageSize">The number of sports to include on each page. Must be greater than 0 if specified. If null, all sports are
    /// returned without pagination.</param>
    /// <returns>An HTTP 200 response containing a collection of sports as <see cref="SportDto"/> objects. If pagination
    /// parameters are provided, returns a paged result with additional pagination metadata. Returns 401 if the user is
    /// unauthorized or 403 if access is forbidden.</returns>
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
    /// Retrieves the details of a sport with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the sport to retrieve.</param>
    /// <returns>An <see cref="ActionResult{T}"/> containing a <see cref="SportDto"/> with the sport details if found; otherwise,
    /// a 404 Not Found response if the sport does not exist. Returns 401 Unauthorized or 403 Forbidden if the caller
    /// does not have sufficient permissions.</returns>
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
    /// Creates a new sport using the specified data.Create a new sport
    /// </summary>
    /// <remarks>Returns a 201 Created response with the details of the newly created sport if the operation
    /// is successful. Returns 400 Bad Request if the input data is invalid, 401 Unauthorized if the user is not
    /// authenticated, or 403 Forbidden if the user does not have permission to create sports.</remarks>
    /// <param name="dto">The data used to create the new sport. Must not be null.</param>
    /// <returns>An <see cref="ActionResult{T}"/> containing the created sport if successful; otherwise, an error response
    /// indicating the reason for failure.</returns>
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
    /// Updates the details of an existing sport with the specified identifier.Update an existing sport
    /// </summary>
    /// <param name="id">The unique identifier of the sport to update.</param>
    /// <param name="dto">An object containing the updated details for the sport. Cannot be null.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns 200 OK if the update is
    /// successful; 404 Not Found if the sport does not exist; 401 Unauthorized or 403 Forbidden if the user is not
    /// authorized.</returns>
    [HttpPut("sports/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateSport(int id, [FromBody] CreateSportDto dto)
    {
        var result = await AdminService.UpdateSportAsync(id, dto);
        if (!result) return NotFound();
        return Ok(new { message = MessageStrings.SportUpdatedSuccessfully });
    }

    /// <summary>
    /// Deletes the sport with the specified identifier.Delete a sport
    /// </summary>
    /// <param name="id">The unique identifier of the sport to delete.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns <see cref="OkResult"/> if the
    /// sport was deleted successfully; <see cref="NotFoundResult"/> if the sport does not exist; or <see
    /// cref="BadRequestObjectResult"/> if the request is invalid.</returns>
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
            return Ok(new { message = MessageStrings.SportUpdatedSuccessfully });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ========== VENUES ==========

    /// <summary>
    ///     Get all venues with optional pagination
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
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
    /// Retrieves the details of a specific venue by its unique identifier.Get a specific venue by ID
    /// </summary>
    /// <param name="id">The unique identifier of the venue to retrieve.</param>
    /// <returns>An <see cref="ActionResult{T}"/> containing a <see cref="VenueDto"/> if the venue is found; otherwise, a 404 Not
    /// Found response.</returns>
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
    /// <param name="dto"></param>
    /// <returns></returns>
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
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("venues/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateVenue(int id, [FromBody] CreateVenueDto dto)
    {
        var result = await AdminService.UpdateVenueAsync(id, dto);
        if (!result) return NotFound();
        return Ok(new { message = MessageStrings.VenueDeletedSuccessfully });
    }

    /// <summary>
    /// Delete a venue
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
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
            return Ok(new { message = MessageStrings.VenueDeletedSuccessfully });
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
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
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
    /// <param name="id"></param>
    /// <returns></returns>
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
    /// <param name="dto"></param>
    /// <returns></returns>
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
            return Ok(new { message = MessageStrings.EmployeeCreatedSuccessfully });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    ///     Update an existing employee
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("employees/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
    {
        var result = await AdminService.UpdateEmployeeAsync(id, dto);
        if (!result) return NotFound();
        return Ok(new { message = MessageStrings.EmployeeUpdatedSuccessfully });
    }

    /// <summary>
    /// Deactivate an employee
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("employees/{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeactivateEmployee(int id)
    {
        var result = await AdminService.DeactivateEmployeeAsync(id);
        if (!result) return NotFound();
        return Ok(new { message = MessageStrings.EmployeeDeactivated });
    }

    /// <summary>
    /// Activate an employee
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("employees/{id}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateEmployee(int id)
    {
        var result = await AdminService.ActivateEmployeeAsync(id);
        if (!result) return NotFound();
        return Ok(new { message = MessageStrings.EmployeeActivated });
    }

    // ========== USERS ==========

    /// <summary>
    /// Get all users with optional pagination
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
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
    ///     Get a specific user by ID
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
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
    ///     Deactivate a user
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpPost("users/{userId}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeactivateUser(string userId)
    {
        var result = await AdminService.DeactivateUserAsync(userId);
        if (!result) return NotFound();
        return Ok(new { message = MessageStrings.UserDeactivated });
    }

    /// <summary>
    ///     Activate a user
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpPost("users/{userId}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateUser(string userId)
    {
        var result = await AdminService.ActivateUserAsync(userId);
        if (!result) return NotFound();
        return Ok(new { message = MessageStrings.UserActivated });
    }

    /// <summary>
    ///     Change a user's role
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
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
            return Ok(new { message = MessageStrings.UserRoleChanged(dto.Role) });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}