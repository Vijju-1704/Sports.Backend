using Sports.Application.DTOs.Admin;
using Sports.Application.DTOs.Games;
using Sports.Domain.Entities;

namespace Sports.Application.Interfaces;

public interface IAdminService
{
    // Sports Management
    Task<IEnumerable<SportDto>> GetAllSportsAsync();
    Task<SportDto?> GetSportByIdAsync(int id);
    Task<SportDto> CreateSportAsync(CreateSportDto dto);
    Task<bool> UpdateSportAsync(int id, CreateSportDto dto); 
    Task<bool> DeleteSportAsync(int id);

    // Venues Management
    Task<IEnumerable<VenueDto>> GetAllVenuesAsync();
    Task<VenueDto?> GetVenueByIdAsync(int id);
    Task<VenueDto> CreateVenueAsync(CreateVenueDto dto);
    Task<bool> UpdateVenueAsync(int id, CreateVenueDto dto);
    Task<bool> DeleteVenueAsync(int id);

    // Employee Management
    Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
    Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
    Task<bool> CreateEmployeeAsync(CreateEmployeeDto dto);
    Task<bool> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto);
    Task<bool> DeactivateEmployeeAsync(int id);
    Task<bool> ActivateEmployeeAsync(int id);

    // User Management
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<bool> DeactivateUserAsync(string userId);
    Task<bool> ActivateUserAsync(string userId);
    Task<bool> ChangeUserRoleAsync(string userId, string role);

    // Statistics
    Task<AdminStatsDto> GetStatisticsAsync();
}