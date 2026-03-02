using System.ComponentModel.DataAnnotations;

namespace Sports.Domain.Entities;

public class AuditLog
{
    [Key]
    public int AuditId { get; set; }
    
    
    public string UserId { get; set; } = string.Empty;
    
    public string? UserName { get; set; }
    
    public string EntityName { get; set; } = string.Empty;
    
    
    public string EntityId { get; set; } = string.Empty;
    
    public string Action { get; set; } = string.Empty;
    
    
    public string? OldValues { get; set; }
    
    
    public string? NewValues { get; set; }
    
    
    public string? ChangedProperties { get; set; }
   
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
   
    public string? IpAddress { get; set; }
}

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
