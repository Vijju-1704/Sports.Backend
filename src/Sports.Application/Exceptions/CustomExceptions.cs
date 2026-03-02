using Sports.Domain.Constants;

namespace Sports.Application.Exceptions;

/// <summary>
/// Thrown when a requested entity is not found in the database
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string entityName, object key) 
        : base(MessageStrings.EntityNotFound(entityName, key)) { }
}

/// <summary>
/// Thrown when validation fails for a DTO or business rule
/// </summary>
public class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }
    
    public ValidationException(Dictionary<string, string[]> errors)
        : base(MessageStrings.ValidationErrorsOccurred)
    {
        Errors = errors;
    }
    
    public ValidationException(string field, string error) : base(MessageStrings.ValidationError)
    {
        Errors = new Dictionary<string, string[]> { { field, new[] { error } } };
    }
}

// Thrown when user is not authorized to perform an action
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = MessageStrings.NotAuthorized)
        : base(message) { }
}

// Thrown when attempting to join a full game
public class GameFullException : Exception
{
    public GameFullException() : base(MessageStrings.GameAlreadyFull) { }
}

// Thrown when attempting to perform action on a cancelled game
public class GameCancelledException : Exception
{
    public GameCancelledException() : base(MessageStrings.ThisGameCancelled) { }
}

// Thrown when user has already joined a game
public class AlreadyJoinedException : Exception
{
    public AlreadyJoinedException() : base(MessageStrings.AlreadyJoinedGame) { }
}

// Thrown when attempting to join a game in the past
public class PastGameException : Exception
{
    public PastGameException() : base(MessageStrings.CannotJoinPastGame) { }
}

// Thrown when duplicate entity exists
public class DuplicateException : Exception
{
    public DuplicateException(string message) : base(message) { }
}

// Thrown when account is locked due to failed login attempts
public class AccountLockedException : Exception
{
    public int MinutesRemaining { get; }
    public AccountLockedException(int minutesRemaining) 
        : base(MessageStrings.AccountLocked(minutesRemaining))
    {
        MinutesRemaining = minutesRemaining;
    }
}

// Thrown when entity is in use and cannot be deleted
public class EntityInUseException : Exception
{
    public EntityInUseException(string message) : base(message) { }
}
