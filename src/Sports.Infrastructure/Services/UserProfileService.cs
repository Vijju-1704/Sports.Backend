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
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserProfileService(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
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
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        user.FullName = dto.FullName;
        // user.Bio = dto.Bio; // Add this field to ApplicationUser entity

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> UpdateProfilePictureAsync(string userId, string pictureUrl)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        // user.ProfilePictureUrl = pictureUrl; // Add this field to ApplicationUser entity

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    // ========== SPORT PROFILES ==========

    public async Task<IEnumerable<UserSportProfileDto>> GetUserSportProfilesAsync(string userId)
    {
        var profiles = await _uow.Repository<UserSportProfile>()
            .GetQueryable()
            .Include(p => p.Sport)
            .Where(p => p.UserId == userId)
            .ToListAsync();

        return profiles.Select(p => new UserSportProfileDto
        {
            ProfileId = p.ProfileId,
            SportId = p.SportId,
            SportName = p.Sport.Name,
            SkillLevel = p.SkillLevel.ToString(),
            ExperienceYears = p.ExperienceYears,
            PreferredPosition = p.PreferredPosition
        });
    }

    public async Task<UserSportProfileDto> AddSportProfileAsync(string userId, CreateUserSportProfileDto dto)
    {
        // Check if already exists
        var existing = await _uow.Repository<UserSportProfile>()
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

        await _uow.Repository<UserSportProfile>().AddAsync(profile);
        await _uow.SaveChangesAsync();

        var sport = await _uow.Repository<Sport>().GetByIdAsync(dto.SportId);

        return new UserSportProfileDto
        {
            ProfileId = profile.ProfileId,
            SportId = profile.SportId,
            SportName = sport?.Name ?? "Unknown",
            SkillLevel = profile.SkillLevel.ToString(),
            ExperienceYears = profile.ExperienceYears,
            PreferredPosition = profile.PreferredPosition
        };
    }

    public async Task<bool> UpdateSportProfileAsync(string userId, int profileId, CreateUserSportProfileDto dto)
    {
        var profile = await _uow.Repository<UserSportProfile>().GetByIdAsync(profileId);
        if (profile == null || profile.UserId != userId) return false;

        var skillLevel = Enum.Parse<SkillLevel>(dto.SkillLevel);

        profile.SkillLevel = skillLevel;
        profile.ExperienceYears = dto.ExperienceYears;
        profile.PreferredPosition = dto.PreferredPosition;

        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveSportProfileAsync(string userId, int profileId)
    {
        var profile = await _uow.Repository<UserSportProfile>().GetByIdAsync(profileId);
        if (profile == null || profile.UserId != userId) return false;

        _uow.Repository<UserSportProfile>().Remove(profile);
        await _uow.SaveChangesAsync();
        return true;
    }

    // ========== STATS ==========

    public async Task<UserStatsDto> GetUserStatsAsync(string userId)
    {
        var gamesHosted = await _uow.Repository<Game>()
            .GetQueryable()
            .CountAsync(g => g.HostUserId == userId);

        var gamesJoined = await _uow.Repository<GameParticipant>()
            .GetQueryable()
            .CountAsync(p => p.UserId == userId);

        var gamesCompleted = await _uow.Repository<GameParticipant>()
            .GetQueryable()
            .Include(p => p.Game)
            .CountAsync(p => p.UserId == userId && p.Game.Status == GameStatus.Completed);

        // Get favorite sports (most played)
        var favoriteSports = await _uow.Repository<GameParticipant>()
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
}