namespace Sports.Application.Exceptions;

/// <summary>
/// Thrown when a requested entity is not found in the database
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string entityName, object key) 
        : base($"{entityName} with id '{key}' was not found.") { }
}

/// <summary>
/// Thrown when validation fails for a DTO or business rule
/// </summary>
public class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }
    
    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
    
    public ValidationException(string field, string error) : base("Validation error.")
    {
        Errors = new Dictionary<string, string[]> { { field, new[] { error } } };
    }
}

/// <summary>
/// Thrown when user is not authorized to perform an action
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "You are not authorized to perform this action.") 
        : base(message) { }
}

/// <summary>
/// Thrown when attempting to join a full game
/// </summary>
public class GameFullException : Exception
{
    public GameFullException() : base("This game is already full.") { }
}

/// <summary>
/// Thrown when attempting to perform action on a cancelled game
/// </summary>
public class GameCancelledException : Exception
{
    public GameCancelledException() : base("This game has been cancelled.") { }
}

/// <summary>
/// Thrown when user has already joined a game
/// </summary>
public class AlreadyJoinedException : Exception
{
    public AlreadyJoinedException() : base("You have already joined this game.") { }
}

/// <summary>
/// Thrown when attempting to join a game in the past
/// </summary>
public class PastGameException : Exception
{
    public PastGameException() : base("Cannot join a game that has already started or ended.") { }
}

/// <summary>
/// Thrown when duplicate entity exists
/// </summary>
public class DuplicateException : Exception
{
    public DuplicateException(string message) : base(message) { }
}

/// <summary>
/// Thrown when account is locked due to failed login attempts
/// </summary>
public class AccountLockedException : Exception
{
    public int MinutesRemaining { get; }
    public AccountLockedException(int minutesRemaining) 
        : base($"Account locked due to multiple failed attempts. Try again in {minutesRemaining} minutes.")
    {
        MinutesRemaining = minutesRemaining;
    }
}

/// <summary>
/// Thrown when entity is in use and cannot be deleted
/// </summary>
public class EntityInUseException : Exception
{
    public EntityInUseException(string message) : base(message) { }
}
