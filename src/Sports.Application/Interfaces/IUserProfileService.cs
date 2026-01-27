using Sports.Application.DTOs.UserProfile;

namespace Sports.Application.Interfaces;

public interface IUserProfileService
{
    Task<UserProfileDto?> GetUserProfileAsync(string userId);
    Task<bool> UpdateProfileAsync(string userId, UpdateUserProfileDto dto);
    Task<bool> UpdateProfilePictureAsync(string userId, string pictureUrl);

    // Sport Profiles
    Task<IEnumerable<UserSportProfileDto>> GetUserSportProfilesAsync(string userId);
    Task<UserSportProfileDto> AddSportProfileAsync(string userId, CreateUserSportProfileDto dto);
    Task<bool> UpdateSportProfileAsync(string userId, int profileId, CreateUserSportProfileDto dto);
    Task<bool> RemoveSportProfileAsync(string userId, int profileId);

    // Stats
    Task<UserStatsDto> GetUserStatsAsync(string userId);

    // Search
    Task<IEnumerable<UserProfileDto>> SearchUsersAsync(string query);
}
