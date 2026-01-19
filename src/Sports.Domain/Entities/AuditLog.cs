using System.ComponentModel.DataAnnotations;

namespace Sports.Domain.Entities;

/// <summary>
/// Audit log entry for tracking changes to entities
/// </summary>
public class AuditLog
{
    [Key]
    public int AuditId { get; set; }
    
    /// <summary>
    /// User who made the change
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Username for display
    /// </summary>
    public string? UserName { get; set; }
    
    /// <summary>
    /// Name of the entity that was changed
    /// </summary>
    public string EntityName { get; set; } = string.Empty;
    
    /// <summary>
    /// Primary key of the entity
    /// </summary>
    public string EntityId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of action: Create, Update, Delete
    /// </summary>
    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON representation of old values (for Update/Delete)
    /// </summary>
    public string? OldValues { get; set; }
    
    /// <summary>
    /// JSON representation of new values (for Create/Update)
    /// </summary>
    public string? NewValues { get; set; }
    
    /// <summary>
    /// Properties that were changed
    /// </summary>
    public string? ChangedProperties { get; set; }
    
    /// <summary>
    /// When the change occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// IP address of the request (optional)
    /// </summary>
    public string? IpAddress { get; set; }
}

/// <summary>
/// Helper class for building audit entries
/// </summary>
public class AuditEntry
{
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Dictionary<string, object?> OldValues { get; } = new();
    public Dictionary<string, object?> NewValues { get; } = new();
    public List<string> ChangedProperties { get; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public AuditLog ToAuditLog()
    {
        return new AuditLog
        {
            UserId = UserId,
            UserName = UserName,
            EntityName = EntityName,
            EntityId = EntityId,
            Action = Action,
            OldValues = OldValues.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(OldValues) : null,
            NewValues = NewValues.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(NewValues) : null,
            ChangedProperties = ChangedProperties.Count > 0 ? string.Join(",", ChangedProperties) : null,
            Timestamp = Timestamp
        };
    }
}
