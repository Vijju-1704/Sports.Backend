using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sports.Application.DTOs.Admin;
using Sports.Application.DTOs.Games;
using Sports.Application.Interfaces;
using Sports.Domain.Entities;
using Sports.Domain.Enums;
using Sports.Domain.Interfaces;
using Sports.Infrastructure.Identity;

namespace Sports.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork Uow;
    private readonly UserManager<ApplicationUser> UserManager;
    private readonly RoleManager<IdentityRole> RoleManager;

    public AdminService(
        IUnitOfWork uow,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        Uow = uow;
        UserManager = userManager;
        RoleManager = roleManager;
    }

    // ========== SPORTS MANAGEMENT ==========

    public async Task<IEnumerable<SportDto>> GetAllSportsAsync()
    {
        var sports = await Uow.Repository<Sport>().GetAllAsync();
        return sports.Select(s => new SportDto
        {
            SportId = s.SportId,
            Name = s.Name,
            Type = s.Type,
            IconUrl = s.IconUrl
        });
    }

    public async Task<SportDto?> GetSportByIdAsync(int id)
    {
        var sport = await Uow.Repository<Sport>().GetByIdAsync(id);
        if (sport == null) return null;

        return new SportDto
        {
            SportId = sport.SportId,
            Name = sport.Name,
            Type = sport.Type,
            IconUrl = sport.IconUrl
        };
    }

    public async Task<SportDto> CreateSportAsync(CreateSportDto dto)
    {
        var sport = new Sport
        {
            Name = dto.Name,
            Type = dto.Type,
            IconUrl = dto.IconUrl
        };

        await Uow.Repository<Sport>().AddAsync(sport);
        await Uow.SaveChangesAsync();

        return new SportDto
        {
            SportId = sport.SportId,
            Name = sport.Name,
            Type = sport.Type,
            IconUrl = sport.IconUrl
        };
    }

    public async Task<bool> UpdateSportAsync(int id, CreateSportDto dto)
    {
        var sport = await Uow.Repository<Sport>().GetByIdAsync(id);
        if (sport == null) return false;

        sport.Name = dto.Name;
        sport.Type = dto.Type;
        sport.IconUrl = dto.IconUrl;

        await Uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteSportAsync(int id)
    {
        var sport = await Uow.Repository<Sport>().GetByIdAsync(id);
        if (sport == null) return false;

        // Check if sport is used in any games
        var gamesUsingSport = await Uow.Repository<Game>()
            .FindAsync(g => g.SportId == id);

        if (gamesUsingSport.Any())
        {
            throw new InvalidOperationException("Cannot delete sport that is being used in games");
        }

        Uow.Repository<Sport>().Remove(sport);
        await Uow.SaveChangesAsync();
        return true;
    }

    // ========== VENUES MANAGEMENT ==========

    public async Task<IEnumerable<VenueDto>> GetAllVenuesAsync()
    {
        var venues = await Uow.Repository<Venue>().GetAllAsync();
        return venues.Select(v => new VenueDto
        {
            VenueId = v.VenueId,
            Name = v.Name,
            Address = v.Address,
            City = v.City,
            MapsUrl = v.MapsUrl,
            Facilities = v.Facilities
        });
    }

    public async Task<VenueDto?> GetVenueByIdAsync(int id)
    {
        var venue = await Uow.Repository<Venue>().GetByIdAsync(id);
        if (venue == null) return null;

        return new VenueDto
        {
            VenueId = venue.VenueId,
            Name = venue.Name,
            Address = venue.Address,
            City = venue.City,
            MapsUrl = venue.MapsUrl,
            Facilities = venue.Facilities
        };
    }

    public async Task<VenueDto> CreateVenueAsync(CreateVenueDto dto)
    {
        var venue = new Venue
        {
            Name = dto.Name,
            Address = dto.Address,
            City = dto.City,
            MapsUrl = dto.MapsUrl,
            Facilities = dto.Facilities
        };

        await Uow.Repository<Venue>().AddAsync(venue);
        await Uow.SaveChangesAsync();

        return new VenueDto
        {
            VenueId = venue.VenueId,
            Name = venue.Name,
            Address = venue.Address,
            City = venue.City,
            MapsUrl = venue.MapsUrl,
            Facilities = venue.Facilities
        };
    }

    public async Task<bool> UpdateVenueAsync(int id, CreateVenueDto dto)
    {
        var venue = await Uow.Repository<Venue>().GetByIdAsync(id);
        if (venue == null) return false;

        venue.Name = dto.Name;
        venue.Address = dto.Address;
        venue.City = dto.City;
        venue.MapsUrl = dto.MapsUrl;
        venue.Facilities = dto.Facilities;

        await Uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteVenueAsync(int id)
    {
        var venue = await Uow.Repository<Venue>().GetByIdAsync(id);
        if (venue == null) return false;

        // Check if venue is used in any games
        var gamesUsingVenue = await Uow.Repository<Game>()
            .FindAsync(g => g.VenueId == id);

        if (gamesUsingVenue.Any())
        {
            throw new InvalidOperationException("Cannot delete venue that is being used in games");
        }

        Uow.Repository<Venue>().Remove(venue);
        await Uow.SaveChangesAsync();
        return true;
    }

    // ========== EMPLOYEE MANAGEMENT ==========

    public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
    {
        var employees = await Uow.Repository<EmployeeDirectory>().GetAllAsync();
        return employees.Select(e => new EmployeeDto
        {
            EmployeeId = e.EmployeeId,
            Email = e.Email,
            EmployeeCode = e.EmployeeCode,
            FullName = e.FullName,
            Department = e.Department,
            Designation = e.Designation,
            IsActive = e.IsActive
        });
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
    {
        var employee = await Uow.Repository<EmployeeDirectory>().GetByIdAsync(id);
        if (employee == null) return null;

        return new EmployeeDto
        {
            EmployeeId = employee.EmployeeId,
            Email = employee.Email,
            EmployeeCode = employee.EmployeeCode,
            FullName = employee.FullName,
            Department = employee.Department,
            Designation = employee.Designation,
            IsActive = employee.IsActive
        };
    }

    public async Task<bool> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        // Check for duplicate email or employee code
        var existing = await Uow.Repository<EmployeeDirectory>()
            .FindAsync(e => e.Email == dto.Email || e.EmployeeCode == dto.EmployeeCode);

        if (existing.Any())
        {
            throw new InvalidOperationException("Employee with this email or code already exists");
        }

        var employee = new EmployeeDirectory
        {
            Email = dto.Email,
            EmployeeCode = dto.EmployeeCode,
            FullName = dto.FullName,
            Department = dto.Department,
            Designation = dto.Designation,
            IsActive = true
        };

        await Uow.Repository<EmployeeDirectory>().AddAsync(employee);
        await Uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto)
    {
        var employee = await Uow.Repository<EmployeeDirectory>().GetByIdAsync(id);
        if (employee == null) return false;

        employee.Email = dto.Email;
        employee.EmployeeCode = dto.EmployeeCode;
        employee.FullName = dto.FullName;
        employee.Department = dto.Department;
        employee.Designation = dto.Designation;

        await Uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateEmployeeAsync(int id)
    {
        var employee = await Uow.Repository<EmployeeDirectory>().GetByIdAsync(id);
        if (employee == null) return false;

        employee.IsActive = false;
        await Uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivateEmployeeAsync(int id)
    {
        var employee = await Uow.Repository<EmployeeDirectory>().GetByIdAsync(id);
        if (employee == null) return false;

        employee.IsActive = true;
        await Uow.SaveChangesAsync();
        return true;
    }

    // ========== USER MANAGEMENT ==========

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = UserManager.Users.ToList();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await UserManager.GetRolesAsync(user);
            var gamesHosted = await Uow.Repository<Game>()
                .GetQueryable()
                .CountAsync(g => g.HostUserId == user.Id);
            var gamesJoined = await Uow.Repository<GameParticipant>()
                .GetQueryable()
                .CountAsync(p => p.UserId == user.Id);

            userDtos.Add(new UserDto
            {
                UserId = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                DateRegistered = user.DateRegistered,
                IsActive = user.LockoutEnd == null || user.LockoutEnd < DateTime.UtcNow,
                Roles = roles.ToList(),
                GamesHosted = gamesHosted,
                GamesJoined = gamesJoined
            });
        }

        return userDtos;
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        var user = await UserManager.FindByIdAsync(userId);
        if (user == null) return null;

        var roles = await UserManager.GetRolesAsync(user);
        var gamesHosted = await Uow.Repository<Game>()
            .GetQueryable()
            .CountAsync(g => g.HostUserId == user.Id);
        var gamesJoined = await Uow.Repository<GameParticipant>()
            .GetQueryable()
            .CountAsync(p => p.UserId == user.Id);

        return new UserDto
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            DateRegistered = user.DateRegistered,
            IsActive = user.LockoutEnd == null || user.LockoutEnd < DateTime.UtcNow,
            Roles = roles.ToList(),
            GamesHosted = gamesHosted,
            GamesJoined = gamesJoined
        };
    }

    public async Task<bool> DeactivateUserAsync(string userId)
    {
        var user = await UserManager.FindByIdAsync(userId);
        if (user == null) return false;

        // Lockout until far future (effectively disable)
        await UserManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        return true;
    }

    public async Task<bool> ActivateUserAsync(string userId)
    {
        var user = await UserManager.FindByIdAsync(userId);
        if (user == null) return false;

        await UserManager.SetLockoutEndDateAsync(user, null);
        return true;
    }

    public async Task<bool> ChangeUserRoleAsync(string userId, string role)
    {
        var user = await UserManager.FindByIdAsync(userId);
        if (user == null) return false;

        if (!await RoleManager.RoleExistsAsync(role))
        {
            throw new InvalidOperationException($"Role '{role}' does not exist");
        }

        var currentRoles = await UserManager.GetRolesAsync(user);
        await UserManager.RemoveFromRolesAsync(user, currentRoles);
        await UserManager.AddToRoleAsync(user, role);

        return true;
    }

    // ========== STATISTICS ==========

    public async Task<AdminStatsDto> GetStatisticsAsync()
    {
        var totalUsers = UserManager.Users.Count();
        var activeUsers = UserManager.Users.Count(u => u.LockoutEnd == null || u.LockoutEnd < DateTime.UtcNow);

        var games = await Uow.Repository<Game>().GetAllAsync();
        var totalGames = games.Count();
        var upcomingGames = games.Count(g => g.DateTime > DateTime.UtcNow && g.Status != GameStatus.Cancelled);

        var sports = await Uow.Repository<Sport>().GetAllAsync();
        var venues = await Uow.Repository<Venue>().GetAllAsync();

        // Games by sport
        var gamesBySport = games
            .GroupBy(g => g.SportId)
            .ToDictionary(
                g => sports.FirstOrDefault(s => s.SportId == g.Key)?.Name ?? "Unknown",
                g => g.Count()
            );

        // Games by month (last 6 months)
        var gamesByMonth = games
            .Where(g => g.DateTime >= DateTime.UtcNow.AddMonths(-6))
            .GroupBy(g => g.DateTime.ToString("MMM yyyy"))
            .ToDictionary(g => g.Key, g => g.Count());

        return new AdminStatsDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            TotalGames = totalGames,
            UpcomingGames = upcomingGames,
            TotalSports = sports.Count(),
            TotalVenues = venues.Count(),
            GamesBySport = gamesBySport,
            GamesByMonth = gamesByMonth
        };
    }
}