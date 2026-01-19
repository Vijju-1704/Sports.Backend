namespace Sports.Domain.Common;

/// <summary>
/// Interface for entities that support soft delete
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Indicates if the entity is deleted
    /// </summary>
    bool IsDeleted { get; set; }
    
    /// <summary>
    /// When the entity was deleted
    /// </summary>
    DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// Who deleted the entity (UserId)
    /// </summary>
    string? DeletedBy { get; set; }
}
