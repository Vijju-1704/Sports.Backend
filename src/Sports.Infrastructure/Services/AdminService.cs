using Sports.Application.DTOs.Games;
using Sports.Application.Interfaces;
using Sports.Domain.Entities;
using Sports.Domain.Interfaces;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _uow;

    public AdminService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    // Sports Management
    public async Task<IEnumerable<SportDto>> GetAllSportsAsync()
    {
        var sports = await _uow.Repository<Sport>().GetAllAsync();
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
        var sport = await _uow.Repository<Sport>().GetByIdAsync(id);
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

        await _uow.Repository<Sport>().AddAsync(sport);
        await _uow.SaveChangesAsync();

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
        var sport = await _uow.Repository<Sport>().GetByIdAsync(id);
        if (sport == null) return false;

        sport.Name = dto.Name;
        sport.Type = dto.Type;
        sport.IconUrl = dto.IconUrl;

        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteSportAsync(int id)
    {
        var sport = await _uow.Repository<Sport>().GetByIdAsync(id);
        if (sport == null) return false;

        _uow.Repository<Sport>().Remove(sport);
        await _uow.SaveChangesAsync();
        return true;
    }

    // Venues Management
    public async Task<IEnumerable<VenueDto>> GetAllVenuesAsync()
    {
        var venues = await _uow.Repository<Venue>().GetAllAsync();
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
        var venue = await _uow.Repository<Venue>().GetByIdAsync(id);
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

        await _uow.Repository<Venue>().AddAsync(venue);
        await _uow.SaveChangesAsync();

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
        var venue = await _uow.Repository<Venue>().GetByIdAsync(id);
        if (venue == null) return false;

        venue.Name = dto.Name;
        venue.Address = dto.Address;
        venue.City = dto.City;
        venue.MapsUrl = dto.MapsUrl;
        venue.Facilities = dto.Facilities;

        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteVenueAsync(int id)
    {
        var venue = await _uow.Repository<Venue>().GetByIdAsync(id);
        if (venue == null) return false;

        _uow.Repository<Venue>().Remove(venue);
        await _uow.SaveChangesAsync();
        return true;
    }

    // Employee Management
    public async Task<IEnumerable<EmployeeDirectory>> GetAllEmployeesAsync()
    {
        return await _uow.Repository<EmployeeDirectory>().GetAllAsync();
    }

    public async Task<EmployeeDirectory?> GetEmployeeByIdAsync(int id)
    {
        return await _uow.Repository<EmployeeDirectory>().GetByIdAsync(id);
    }

    public async Task<bool> CreateEmployeeAsync(EmployeeDirectory employee)
    {
        await _uow.Repository<EmployeeDirectory>().AddAsync(employee);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateEmployeeAsync(int id, EmployeeDirectory employee)
    {
        var existing = await _uow.Repository<EmployeeDirectory>().GetByIdAsync(id);
        if (existing == null) return false;

        existing.Email = employee.Email;
        existing.EmployeeCode = employee.EmployeeCode;
        existing.FullName = employee.FullName;
        existing.Department = employee.Department;
        existing.Designation = employee.Designation;
        existing.IsActive = employee.IsActive;

        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        var employee = await _uow.Repository<EmployeeDirectory>().GetByIdAsync(id);
        if (employee == null) return false;

        _uow.Repository<EmployeeDirectory>().Remove(employee);
        await _uow.SaveChangesAsync();
        return true;
    }
}