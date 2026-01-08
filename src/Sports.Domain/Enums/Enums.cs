namespace Sports.Domain.Enums;

public enum SkillLevel
{
    Beginner = 1,
    Intermediate = 2,
    Advanced = 3,
    Expert = 4
}

public enum GameStatus
{
    Open = 1,
    Full = 2,
    Completed = 3,
    Cancelled = 4,
    InProgress = 5
}

public enum JoinStatus
{
    Confirmed = 1,
    Waitlist = 2
}

public enum RequestStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum NotificationType
{
    Info = 1,
    Alert = 2,
    Request = 3,
    JoinRequest = 4,
    GameCancelled = 5
}
