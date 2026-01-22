namespace Sports.Application.DTOs.UserProfile;

public class UserProfileDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime DateRegistered { get; set; }
    public List<UserSportProfileDto> SportProfiles { get; set; } = new();
    public UserStatsDto Stats { get; set; } = new();
}

public class UpdateUserProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
}

public class UserSportProfileDto
{
    public int ProfileId { get; set; }
    public int SportProfileId { get; set; } // Alias for ProfileId
    public int SportId { get; set; }
    public string SportName { get; set; } = string.Empty;
    public string SportIcon { get; set; } = string.Empty;
    public string SkillLevel { get; set; } = string.Empty;
    public int SkillLevelValue { get; set; } // Numeric 1-5
    public int ExperienceYears { get; set; }
    public string? PreferredPosition { get; set; }
}

public class CreateUserSportProfileDto
{
    public int SportId { get; set; }
    public string SkillLevel { get; set; } = string.Empty; // Beginner, Intermediate, Advanced, Expert
    public int ExperienceYears { get; set; }
    public string? PreferredPosition { get; set; }
}

public class UserStatsDto
{
    public int GamesHosted { get; set; }
    public int GamesJoined { get; set; }
    public int GamesCompleted { get; set; }
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public List<string> FavoriteSports { get; set; } = new();
}