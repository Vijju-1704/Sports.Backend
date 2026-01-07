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
    Task<IEnumerable<EmployeeDirectory>> GetAllEmployeesAsync();
    Task<EmployeeDirectory?> GetEmployeeByIdAsync(int id);
    Task<bool> CreateEmployeeAsync(EmployeeDirectory employee);
    Task<bool> UpdateEmployeeAsync(int id, EmployeeDirectory employee);
    Task<bool> DeleteEmployeeAsync(int id);
}