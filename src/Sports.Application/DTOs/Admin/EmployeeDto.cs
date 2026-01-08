using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sports.Application.DTOs.Admin
{
    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CreateEmployeeDto
    {
        public string Email { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
    }

    public class UpdateEmployeeDto : CreateEmployeeDto
    {
    }

    // DTOs for User Management
    public class UserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime DateRegistered { get; set; }
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
        public int GamesHosted { get; set; }
        public int GamesJoined { get; set; }
    }

    public class AdminStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalGames { get; set; }
        public int UpcomingGames { get; set; }
        public int TotalSports { get; set; }
        public int TotalVenues { get; set; }
        public Dictionary<string, int> GamesBySport { get; set; } = new();
        public Dictionary<string, int> GamesByMonth { get; set; } = new();
    }
    public class ChangeRoleDto
    {
        public string Role { get; set; } = string.Empty;
    }
}
