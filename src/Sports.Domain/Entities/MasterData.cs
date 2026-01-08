using System.ComponentModel.DataAnnotations;
namespace Sports.Domain.Entities;

public class Sport
{
    [Key]
    public int SportId { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty; // e.g., Cricket
    public string Type { get; set; } = string.Empty; // e.g., Team
    public string? IconUrl { get; set; }
}

public class Venue
{
    [Key]
    public int VenueId { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty; 
    public string? MapsUrl { get; set; }
    public string? Facilities { get; set; } // Comma separated: "Parking,Showers"
}

public class EmployeeDirectory
{
    [Key]
    public int EmployeeId { get; set; }
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
