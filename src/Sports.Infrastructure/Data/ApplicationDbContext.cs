using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sports.Domain.Entities;
using Sports.Infrastructure.Identity;

namespace Sports.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Sport> Sports { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<EmployeeDirectory> EmployeeDirectory { get; set; }
    public DbSet<UserEmployeeMap> UserEmployeeMap { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<GameParticipant> GameParticipants { get; set; }
    public DbSet<UserSportProfile> UserSportProfiles { get; set; }
    public DbSet<JoinRequest> JoinRequests { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Seed Sports
        builder.Entity<Sport>().HasData(
            new Sport { SportId = 1, Name = "Cricket", Type = "Team", IconUrl = "🏏" },
            new Sport { SportId = 2, Name = "Badminton", Type = "Individual", IconUrl = "🏸" },
            new Sport { SportId = 3, Name = "Football", Type = "Team", IconUrl = "⚽" },
            new Sport { SportId = 4, Name = "Table Tennis", Type = "Individual", IconUrl = "🏓" }
        );

        // Seed Venues
        builder.Entity<Venue>().HasData(
            new Venue { VenueId = 1, Name = "Central Ground", City = "Hyderabad" },
            new Venue { VenueId = 2, Name = "Corporate Arena", City = "Bangalore" }
        );

        // Seed Employee Directory
        builder.Entity<EmployeeDirectory>().HasData(
            new EmployeeDirectory { EmployeeId = 1, EmployeeCode = "EMP001", Email = "john.doe@techcorp.com", FullName = "John Doe", Department = "IT", IsActive = true },
            new EmployeeDirectory { EmployeeId = 2, EmployeeCode = "EMP002", Email = "jane.smith@techcorp.com", FullName = "Jane Smith", Department = "HR", IsActive = true },
             new EmployeeDirectory { EmployeeId = 3, EmployeeCode = "EMP003", Email = "admin@techcorp.com", FullName = "System Admin", Department = "Admin", IsActive = true }
        );
    }
}
