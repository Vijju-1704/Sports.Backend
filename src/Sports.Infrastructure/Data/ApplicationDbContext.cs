using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.AspNetCore.Http;
using Sports.Domain.Common;
using Sports.Domain.Entities;
using Sports.Infrastructure.Identity;
using System.Linq.Expressions;
using System.Text.Json;

namespace Sports.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly IHttpContextAccessor? HttpContextAccessor;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options, 
        IHttpContextAccessor httpContextAccessor) : base(options)
    {
        HttpContextAccessor = httpContextAccessor;
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
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply soft delete filter to all ISoftDeletable entities
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var falseConstant = Expression.Constant(false);
                var condition = Expression.Equal(property, falseConstant);
                var lambda = Expression.Lambda(condition, parameter);
                
                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

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
            new EmployeeDirectory { EmployeeId = 1, EmployeeCode = "EMP001", Email = "vijay.rakesh@techcorp.com", FullName = "Vijay Rakesh", Department = "IT", IsActive = true },
            new EmployeeDirectory { EmployeeId = 2, EmployeeCode = "EMP002", Email = "virat.kohli@techcorp.com", FullName = "Virat Kohli", Department = "HR", IsActive = true },
            new EmployeeDirectory { EmployeeId = 3, EmployeeCode = "EMP003", Email = "admin@techcorp.com", FullName = "System Admin", Department = "Admin", IsActive = true }
        );
    }

    /// <summary>
    /// Override SaveChangesAsync to implement soft delete and audit trail
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = HttpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            // Handle soft delete
            if (entry.Entity is ISoftDeletable softDeletable && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                softDeletable.IsDeleted = true;
                softDeletable.DeletedAt = DateTime.UtcNow;
                softDeletable.DeletedBy = userId;
            }

            // Track changes for audit (skip AuditLog itself)
            if (entry.Entity.GetType() == typeof(AuditLog))
                continue;

            if (entry.State == EntityState.Added ||
                entry.State == EntityState.Modified ||
                entry.State == EntityState.Deleted)
            {
                var auditEntry = CreateAuditEntry(entry, userId);
                if (auditEntry != null)
                {
                    auditEntries.Add(auditEntry);
                }
            }
        }

        // Save audit logs
        if (auditEntries.Any())
        {
            foreach (var auditEntry in auditEntries)
            {
                AuditLogs.Add(auditEntry.ToAuditLog());
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private AuditEntry? CreateAuditEntry(EntityEntry entry, string userId)
    {
        var entityName = entry.Entity.GetType().Name;
        
        // Skip tracking for certain entities
        var excludedEntities = new[] { "AuditLog", "ChatMessage" };
        if (excludedEntities.Contains(entityName))
            return null;

        var keyProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
        var entityId = keyProperty?.CurrentValue?.ToString() ?? "0";

        var auditEntry = new AuditEntry
        {
            UserId = userId,
            EntityName = entityName,
            EntityId = entityId,
            Action = entry.State.ToString()
        };

        foreach (var property in entry.Properties)
        {
            if (property.IsTemporary)
                continue;

            var propertyName = property.Metadata.Name;

            switch (entry.State)
            {
                case EntityState.Added:
                    auditEntry.NewValues[propertyName] = property.CurrentValue;
                    break;

                case EntityState.Deleted:
                    auditEntry.OldValues[propertyName] = property.OriginalValue;
                    break;

                case EntityState.Modified:
                    if (property.IsModified && !Equals(property.OriginalValue, property.CurrentValue))
                    {
                        auditEntry.ChangedProperties.Add(propertyName);
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                    }
                    break;
            }
        }

        // Only track if there are actual changes
        if (entry.State == EntityState.Modified && !auditEntry.ChangedProperties.Any())
            return null;

        return auditEntry;
    }
}
