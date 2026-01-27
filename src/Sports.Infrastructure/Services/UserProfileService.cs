using Sports.Application.DTOs.UserProfile;
using Sports.Application.Interfaces;
using Sports.Domain.Entities;
using Sports.Domain.Interfaces;
using Sports.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Sports.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Sports.Infrastructure.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUnitOfWork Uow;
    private readonly UserManager<ApplicationUser> UserManager;

    public UserProfileService(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        Uow = uow;
        UserManager = userManager;
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
    {
        var user = await UserManager.FindByIdAsync(userId);
        if (user == null) return null;

        var sportProfiles = await GetUserSportProfilesAsync(userId);
        var stats = await GetUserStatsAsync(userId);

        return new UserProfileDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Bio = null, // Will be added when we add Bio field to ApplicationUser
            ProfilePictureUrl = null, // Will be added for profile pictures
            DateRegistered = user.DateRegistered,
            SportProfiles = sportProfiles.ToList(),
            Stats = stats
        };
    }

    public async Task<bool> UpdateProfileAsync(string userId, UpdateUserProfileDto dto)
    {
        var user = await UserManager.FindByIdAsync(userId);
        if (user == null) return false;

        user.FullName = dto.FullName;
        // user.Bio = dto.Bio;

        var result = await UserManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> UpdateProfilePictureAsync(string userId, string pictureUrl)
    {
        var user = await UserManager.FindByIdAsync(userId);
        if (user == null) return false;

        // user.ProfilePictureUrl = pictureUrl;

        var result = await UserManager.UpdateAsync(user);
        return result.Succeeded;
    }

    // ========== SPORT PROFILES ==========

    public async Task<IEnumerable<UserSportProfileDto>> GetUserSportProfilesAsync(string userId)
    {
        var profiles = await Uow.Repository<UserSportProfile>()
            .GetQueryable()
            .Include(p => p.Sport)
            .Where(p => p.UserId == userId)
            .ToListAsync();

        return profiles.Select(p => new UserSportProfileDto
        {
            ProfileId = p.ProfileId,
            SportProfileId = p.ProfileId, // Alias
            SportId = p.SportId,
            SportName = p.Sport.Name,
            SportIcon = p.Sport.IconUrl ?? "??",
            SkillLevel = p.SkillLevel.ToString(),
            SkillLevelValue = (int)p.SkillLevel,
            ExperienceYears = p.ExperienceYears,
            PreferredPosition = p.PreferredPosition
        });
    }

    public async Task<UserSportProfileDto> AddSportProfileAsync(string userId, CreateUserSportProfileDto dto)
    {
        // Check if already exists
        var existing = await Uow.Repository<UserSportProfile>()
            .GetQueryable()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.SportId == dto.SportId);

        if (existing != null)
        {
            throw new InvalidOperationException("Sport profile already exists for this sport");
        }

        var skillLevel = Enum.Parse<SkillLevel>(dto.SkillLevel);

        var profile = new UserSportProfile
        {
            UserId = userId,
            SportId = dto.SportId,
            SkillLevel = skillLevel,
            ExperienceYears = dto.ExperienceYears,
            PreferredPosition = dto.PreferredPosition
        };

        await Uow.Repository<UserSportProfile>().AddAsync(profile);
        await Uow.SaveChangesAsync();

        var sport = await Uow.Repository<Sport>().GetByIdAsync(dto.SportId);

        return new UserSportProfileDto
        {
            ProfileId = profile.ProfileId,
            SportProfileId = profile.ProfileId,
            SportId = profile.SportId,
            SportName = sport?.Name ?? "Unknown",
            SportIcon = sport?.IconUrl ?? "??",
            SkillLevel = profile.SkillLevel.ToString(),
            SkillLevelValue = (int)profile.SkillLevel,
            ExperienceYears = profile.ExperienceYears,
            PreferredPosition = profile.PreferredPosition
        };
    }

    public async Task<bool> UpdateSportProfileAsync(string userId, int profileId, CreateUserSportProfileDto dto)
    {
        var profile = await Uow.Repository<UserSportProfile>().GetByIdAsync(profileId);
        if (profile == null || profile.UserId != userId) return false;

        var skillLevel = Enum.Parse<SkillLevel>(dto.SkillLevel);

        profile.SkillLevel = skillLevel;
        profile.ExperienceYears = dto.ExperienceYears;
        profile.PreferredPosition = dto.PreferredPosition;

        await Uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveSportProfileAsync(string userId, int profileId)
    {
        var profile = await Uow.Repository<UserSportProfile>().GetByIdAsync(profileId);
        if (profile == null || profile.UserId != userId) return false;

        Uow.Repository<UserSportProfile>().Remove(profile);
        await Uow.SaveChangesAsync();
        return true;
    }

    // ========== STATS ==========

    public async Task<UserStatsDto> GetUserStatsAsync(string userId)
    {
        var gamesHosted = await Uow.Repository<Game>()
            .GetQueryable()
            .CountAsync(g => g.HostUserId == userId);

        var gamesJoined = await Uow.Repository<GameParticipant>()
            .GetQueryable()
            .CountAsync(p => p.UserId == userId);

        var gamesCompleted = await Uow.Repository<GameParticipant>()
            .GetQueryable()
            .Include(p => p.Game)
            .CountAsync(p => p.UserId == userId && p.Game.Status == GameStatus.Completed);

        // Get favorite sports (most played)
        var favoriteSports = await Uow.Repository<GameParticipant>()
            .GetQueryable()
            .Include(p => p.Game)
            .ThenInclude(g => g.Sport)
            .Where(p => p.UserId == userId)
            .GroupBy(p => p.Game.Sport.Name)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToListAsync();

        return new UserStatsDto
        {
            GamesHosted = gamesHosted,
            GamesJoined = gamesJoined,
            GamesCompleted = gamesCompleted,
            AverageRating = 0, // Will be calculated when rating system is added
            TotalRatings = 0,
            FavoriteSports = favoriteSports
        };
    }
    public async Task<IEnumerable<UserProfileDto>> SearchUsersAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return new List<UserProfileDto>();

        var normalizedQuery = query.ToUpper();

        var users = await UserManager.Users
            .Where(u => u.FullName.ToUpper().Contains(normalizedQuery) || 
                        (u.Email != null && u.Email.ToUpper().Contains(normalizedQuery)))
            .Take(10)
            .ToListAsync();

        return users.Select(u => new UserProfileDto
        {
            UserId = u.Id,
            FullName = u.FullName,
            Email = u.Email ?? "",
            ProfilePictureUrl = null,
            Bio = null,
            DateRegistered = u.DateRegistered
        });
    }
}
