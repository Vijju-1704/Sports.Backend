using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sports.Application.DTOs.Ratings;
using Sports.Application.Interfaces;
using Sports.Domain.Constants;
using Sports.Domain.Entities;
using Sports.Domain.Enums;
using Sports.Domain.Interfaces;
using Sports.Infrastructure.Identity;

namespace Sports.Infrastructure.Services;

public class RatingService : IRatingService
{
    private readonly IUnitOfWork Uow;
    private readonly UserManager<ApplicationUser> UserManager;
    private readonly INotificationService NotificationService;

    public RatingService(
        IUnitOfWork uow,
        UserManager<ApplicationUser> userManager,
        INotificationService notificationService)
    {
        Uow = uow;
        UserManager = userManager;
        NotificationService = notificationService;
    }

    public async Task<RatePlayersDto?> GetPlayersToRateAsync(int gameId, string raterId)
    {
        var game = await Uow.Repository<Game>()
            .GetQueryable()
            .Include(g => g.Sport)
            .Include(g => g.Venue)
            .Include(g => g.Participants)
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null || game.Status != GameStatus.Completed)
            return null;

        // Check if user was a participant
        if (!game.Participants.Any(p => p.UserId == raterId))
            return null;

        var host = await UserManager.FindByIdAsync(game.HostUserId);
        
        // Get existing ratings by this user for this game
        var existingRatings = await Uow.Repository<PlayerRating>()
            .FindAsync(r => r.GameId == gameId && r.RaterId == raterId);
        var ratedUserIds = existingRatings.Select(r => r.RatedUserId).ToHashSet();

        // Build players to rate list (exclude self and already rated)
        var playersToRate = new List<PlayerToRateDto>();
        
        foreach (var participant in game.Participants.Where(p => p.UserId != raterId && !ratedUserIds.Contains(p.UserId)))
        {
            var user = await UserManager.FindByIdAsync(participant.UserId);
            if (user == null) continue;

            // Get player's current skill level for this sport
            var sportProfile = await Uow.Repository<UserSportProfile>()
                .FindAsync(sp => sp.UserId == participant.UserId && sp.SportId == game.SportId);
            var currentSkill = sportProfile.FirstOrDefault()?.SkillLevel.ToString();

            playersToRate.Add(new PlayerToRateDto
            {
                UserId = participant.UserId,
                UserName = user.FullName,
                IsHost = participant.UserId == game.HostUserId,
                CurrentSkillLevel = currentSkill
            });
        }

        return new RatePlayersDto
        {
            GameId = game.GameId,
            GameTitle = game.Title,
            SportName = game.Sport.Name,
            SportIcon = game.Sport.IconUrl ?? "🎮",
            GameDate = game.DateTime,
            VenueName = game.Venue.Name,
            HostName = host?.FullName ?? MessageStrings.Unknown,
            PlayersToRate = playersToRate
        };
    }

    public async Task<bool> SubmitRatingsAsync(int gameId, string raterId, SubmitRatingsDto dto)
    {
        var game = await Uow.Repository<Game>()
            .GetQueryable()
            .Include(g => g.Participants)
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null || game.Status != GameStatus.Completed)
            return false;

        // Verify rater was a participant
        if (!game.Participants.Any(p => p.UserId == raterId))
            return false;

        foreach (var rating in dto.Ratings)
        {
            // Skip self-ratings and non-participants
            if (rating.UserId == raterId)
                continue;
            
            if (!game.Participants.Any(p => p.UserId == rating.UserId))
                continue;

            // Check if already rated
            var existing = await Uow.Repository<PlayerRating>()
                .FindAsync(r => r.GameId == gameId && r.RaterId == raterId && r.RatedUserId == rating.UserId);
            
            if (existing.Any())
                continue;

            var playerRating = new PlayerRating
            {
                GameId = gameId,
                RaterId = raterId,
                RatedUserId = rating.UserId,
                SkillRating = Math.Clamp(rating.SkillRating, 1, 5),
                WasOnTime = rating.WasOnTime,
                Comments = rating.Comments,
                RatedAt = DateTime.UtcNow
            };

            await Uow.Repository<PlayerRating>().AddAsync(playerRating);
        }

        await Uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasUserRatedAsync(int gameId, string userId)
    {
        var ratings = await Uow.Repository<PlayerRating>()
            .FindAsync(r => r.GameId == gameId && r.RaterId == userId);
        
        return ratings.Any();
    }

    public async Task<GameRatingsDto?> GetGameRatingsAsync(int gameId)
    {
        var game = await Uow.Repository<Game>()
            .GetQueryable()
            .Include(g => g.Sport)
            .Include(g => g.Venue)
            .Include(g => g.Participants)
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null)
            return null;

        var host = await UserManager.FindByIdAsync(game.HostUserId);
        
        var allRatings = await Uow.Repository<PlayerRating>()
            .FindAsync(r => r.GameId == gameId);
        
        var allRatingsList = allRatings.ToList();

        // Build rating results
        var ratingResults = new List<PlayerRatingResultDto>();
        foreach (var rating in allRatingsList)
        {
            var rater = await UserManager.FindByIdAsync(rating.RaterId);
            var ratedUser = await UserManager.FindByIdAsync(rating.RatedUserId);

            ratingResults.Add(new PlayerRatingResultDto
            {
                RatingId = rating.PlayerRatingId,
                RaterName = rater?.FullName ?? MessageStrings.Unknown,
                RatedPlayerName = ratedUser?.FullName ?? MessageStrings.Unknown,
                SkillRating = rating.SkillRating,
                WasOnTime = rating.WasOnTime,
                Comments = rating.Comments,
                RatedAt = rating.RatedAt
            });
        }

        // Build player summaries
        var playerSummaries = new List<PlayerSummaryDto>();
        var ratedUserIds = allRatingsList.Select(r => r.RatedUserId).Distinct().ToList();

        foreach (var ratedUserId in ratedUserIds)
        {
            var userRatings = allRatingsList.Where(r => r.RatedUserId == ratedUserId).ToList();
            var user = await UserManager.FindByIdAsync(ratedUserId);

            playerSummaries.Add(new PlayerSummaryDto
            {
                UserId = ratedUserId,
                PlayerName = user?.FullName ?? MessageStrings.Unknown,
                IsHost = ratedUserId == game.HostUserId,
                AverageRating = userRatings.Average(r => r.SkillRating),
                WasOnTimePercentage = userRatings.Count > 0 
                    ? (double)userRatings.Count(r => r.WasOnTime) / userRatings.Count * 100 
                    : 0,
                RatingsReceived = userRatings.Count,
                MostCommonComment = userRatings
                    .Where(r => !string.IsNullOrEmpty(r.Comments))
                    .GroupBy(r => r.Comments)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key
            });
        }

        return new GameRatingsDto
        {
            GameId = game.GameId,
            GameTitle = game.Title,
            SportName = game.Sport.Name,
            SportIcon = game.Sport.IconUrl ?? "🎮",
            GameDate = game.DateTime,
            VenueName = game.Venue.Name,
            HostName = host?.FullName ?? MessageStrings.Unknown,
            TotalParticipants = game.Participants.Count,
            TotalRatingsSubmitted = allRatingsList.Count,
            AverageSkillRating = allRatingsList.Count > 0 ? allRatingsList.Average(r => r.SkillRating) : 0,
            OnTimePercentage = allRatingsList.Count > 0 
                ? (double)allRatingsList.Count(r => r.WasOnTime) / allRatingsList.Count * 100 
                : 0,
            PlayersRated = ratedUserIds.Count,
            PlayerSummaries = playerSummaries,
            AllRatings = ratingResults
        };
    }

    public async Task SendRatingNotificationsAsync(int gameId)
    {
        var game = await Uow.Repository<Game>()
            .GetQueryable()
            .Include(g => g.Participants)
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null)
            return;

        foreach (var participant in game.Participants)
        {
            await NotificationService.CreateNotificationAsync(
                participant.UserId,
                MessageStrings.GameCompletedRate(game.Title),
                NotificationType.RatingRequest
            );
        }
    }
}
