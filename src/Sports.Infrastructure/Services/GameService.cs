using Sports.Application.DTOs.Games;
using Sports.Application.Interfaces;
using Sports.Domain.Entities;
using Sports.Domain.Interfaces;
using Sports.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Sports.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Sports.Infrastructure.Services;

public class GameService : IGameService
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IGenericRepository<Sport> _sportRepo;
    private readonly IGenericRepository<Venue> _venueRepo;
    private readonly INotificationService _notificationService;

    public GameService(IUnitOfWork uow, UserManager<ApplicationUser> userManager, INotificationService notificationService)
    {
        _uow = uow;
        _userManager = userManager;
        _notificationService = notificationService;
        _sportRepo = _uow.Repository<Sport>();
        _venueRepo = _uow.Repository<Venue>();
    }

    // ✅ IMPROVED: Advanced search with filters
    public async Task<IEnumerable<GameDto>> GetAllGamesAsync(
        string? sport = null,
        DateTime? date = null,
        int? sportId = null,
        string? city = null)
    {
        var query = _uow.Repository<Game>().GetQueryable();

        // Text search in title or sport name
        if (!string.IsNullOrEmpty(sport))
        {
            query = query.Where(g => g.Title.Contains(sport) || g.Sport.Name.Contains(sport));
        }

        // Filter by date
        if (date.HasValue)
        {
            query = query.Where(g => g.DateTime.Date == date.Value.Date);
        }

        // Filter by specific sport
        if (sportId.HasValue)
        {
            query = query.Where(g => g.SportId == sportId.Value);
        }

        // Filter by city
        if (!string.IsNullOrEmpty(city))
        {
            query = query.Where(g => g.Venue.City.Contains(city));
        }

        var games = await query
            .Include(g => g.Participants)
            .Include(g => g.Sport)
            .Include(g => g.Venue)
            .Where(g => g.Status != GameStatus.Cancelled) // Don't show cancelled games
            .OrderByDescending(g => g.DateTime)
            .ToListAsync();

        var gameDtos = new List<GameDto>();

        foreach (var g in games)
        {
            var host = await _userManager.FindByIdAsync(g.HostUserId);

            gameDtos.Add(new GameDto
            {
                GameId = g.GameId,
                Title = g.Title,
                SportId = g.SportId,
                HostName = host?.FullName ?? "Unknown",
                HostUserId = g.HostUserId,
                VenueId = g.VenueId,
                VenueName = g.Venue.Name,
                DateTime = g.DateTime,
                DurationMinutes = g.DurationMinutes,
                MaxPlayers = g.MaxPlayers,
                MinPlayers = g.MinPlayers,
                PlayerCount = g.Participants.Count,
                Status = g.Status.ToString(),
                CostPerPerson = g.CostPerPerson,
                SportName = g.Sport.Name,
                SportIcon = g.Sport.IconUrl ?? ""
            });
        }

        return gameDtos;
    }

    public async Task<GameDetailDto?> GetGameByIdAsync(int id)
    {
        var game = await _uow.Repository<Game>()
            .GetQueryable()
            .Include(g => g.Sport)
            .Include(g => g.Venue)
            .Include(g => g.Participants)
            .FirstOrDefaultAsync(g => g.GameId == id);

        if (game == null) return null;

        var host = await _userManager.FindByIdAsync(game.HostUserId);

        var participants = new List<ParticipantDto>();
        foreach (var p in game.Participants)
        {
            var user = await _userManager.FindByIdAsync(p.UserId);
            participants.Add(new ParticipantDto
            {
                UserId = p.UserId,
                UserName = user?.FullName ?? "Unknown",
                Status = p.Status.ToString(),
                JoinedAt = p.JoinedAt,
                IsHost = p.UserId == game.HostUserId
            });
        }

        return new GameDetailDto
        {
            GameId = game.GameId,
            Title = game.Title,
            SportId = game.SportId,
            SportName = game.Sport.Name,
            SportIcon = game.Sport.IconUrl ?? "",
            VenueId = game.VenueId,
            VenueName = game.Venue.Name,
            DateTime = game.DateTime,
            DurationMinutes = game.DurationMinutes,
            MaxPlayers = game.MaxPlayers,
            MinPlayers = game.MinPlayers,
            CostPerPerson = game.CostPerPerson,
            Status = game.Status.ToString(),
            HostName = host?.FullName ?? "Unknown",
            HostUserId = game.HostUserId,
            EquipmentNeeded = game.EquipmentNeeded,
            PlayerCount = game.Participants.Count,
            Participants = participants
        };
    }

    public async Task<GameDto> CreateGameAsync(CreateGameDto createDto, string hostUserId)
    {
        if (createDto.MaxPlayers < createDto.MinPlayers)
        {
            throw new ArgumentException("MaxPlayers must be greater than or equal to MinPlayers");
        }

        var game = new Game
        {
            Title = createDto.Title,
            SportId = createDto.SportId,
            VenueId = createDto.VenueId,
            HostUserId = hostUserId,
            DateTime = createDto.DateTime,
            DurationMinutes = createDto.DurationMinutes,
            MinPlayers = createDto.MinPlayers,
            MaxPlayers = createDto.MaxPlayers,
            CostPerPerson = createDto.CostPerPerson,
            EquipmentNeeded = createDto.EquipmentNeeded,
            Status = GameStatus.Open
        };

        await _uow.Repository<Game>().AddAsync(game);
        await _uow.SaveChangesAsync();

        var participant = new GameParticipant
        {
            GameId = game.GameId,
            UserId = hostUserId,
            Status = JoinStatus.Confirmed,
            JoinedAt = DateTime.UtcNow
        };
        await _uow.Repository<GameParticipant>().AddAsync(participant);
        await _uow.SaveChangesAsync();

        var result = await GetGameByIdAsync(game.GameId);
        return new GameDto
        {
            GameId = result!.GameId,
            Title = result.Title,
            SportId = result.SportId,
            SportName = result.SportName,
            SportIcon = result.SportIcon,
            VenueId = result.VenueId,
            VenueName = result.VenueName,
            DateTime = result.DateTime,
            DurationMinutes = result.DurationMinutes,
            MaxPlayers = result.MaxPlayers,
            MinPlayers = result.MinPlayers,
            PlayerCount = result.PlayerCount,
            Status = result.Status,
            CostPerPerson = result.CostPerPerson,
            HostName = result.HostName,
            HostUserId = result.HostUserId
        };
    }

    public async Task<bool> JoinGameAsync(int gameId, string userId)
    {
        var game = await _uow.Repository<Game>()
            .GetQueryable()
            .Include(g => g.Participants)
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null || game.Status != GameStatus.Open) return false;
        if (game.Participants.Any(p => p.UserId == userId)) return false;
        if (game.Participants.Count >= game.MaxPlayers) return false;

        var participant = new GameParticipant
        {
            GameId = gameId,
            UserId = userId,
            Status = JoinStatus.Confirmed,
            JoinedAt = DateTime.UtcNow
        };

        await _uow.Repository<GameParticipant>().AddAsync(participant);

        if (game.Participants.Count + 1 >= game.MaxPlayers)
        {
            game.Status = GameStatus.Full;
        }

        await _uow.SaveChangesAsync();

        var user = await _userManager.FindByIdAsync(userId);
        await _notificationService.CreateNotificationAsync(
            game.HostUserId,
            $"{user?.FullName ?? "Someone"} joined your game '{game.Title}'",
            NotificationType.JoinRequest
        );

        return true;
    }

    // ✅ NEW: Leave Game Feature
    public async Task<bool> LeaveGameAsync(int gameId, string userId)
    {
        var game = await _uow.Repository<Game>()
            .GetQueryable()
            .Include(g => g.Participants)
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null) return false;

        // Host cannot leave their own game
        if (game.HostUserId == userId)
        {
            return false;
        }

        var participant = game.Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant == null) return false;

        _uow.Repository<GameParticipant>().Remove(participant);

        // If game was full, reopen it
        if (game.Status == GameStatus.Full && game.Participants.Count - 1 < game.MaxPlayers)
        {
            game.Status = GameStatus.Open;
        }

        await _uow.SaveChangesAsync();

        // Notify host
        var user = await _userManager.FindByIdAsync(userId);
        await _notificationService.CreateNotificationAsync(
            game.HostUserId,
            $"{user?.FullName ?? "Someone"} left your game '{game.Title}'",
            NotificationType.Info
        );

        return true;
    }

    public async Task<bool> UpdateGameAsync(int gameId, CreateGameDto updateDto, string userId)
    {
        var game = await _uow.Repository<Game>().GetByIdAsync(gameId);
        if (game == null) return false;
        if (game.HostUserId != userId) return false;

        if (updateDto.MaxPlayers < updateDto.MinPlayers)
        {
            throw new ArgumentException("MaxPlayers must be greater than or equal to MinPlayers");
        }

        game.Title = updateDto.Title;
        game.SportId = updateDto.SportId;
        game.VenueId = updateDto.VenueId;
        game.DateTime = updateDto.DateTime;
        game.DurationMinutes = updateDto.DurationMinutes;
        game.MinPlayers = updateDto.MinPlayers;
        game.MaxPlayers = updateDto.MaxPlayers;
        game.CostPerPerson = updateDto.CostPerPerson;
        game.EquipmentNeeded = updateDto.EquipmentNeeded;

        await _uow.SaveChangesAsync();

        var participants = await _uow.Repository<GameParticipant>()
            .FindAsync(p => p.GameId == gameId && p.UserId != userId);

        foreach (var p in participants)
        {
            await _notificationService.CreateNotificationAsync(
                p.UserId,
                $"Game '{game.Title}' has been updated by the host.",
                NotificationType.Info
            );
        }

        return true;
    }

    public async Task<bool> CancelGameAsync(int gameId, string userId)
    {
        var game = await _uow.Repository<Game>()
            .GetQueryable()
            .Include(g => g.Participants)
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null) return false;
        if (game.HostUserId != userId) return false;

        game.Status = GameStatus.Cancelled;
        await _uow.SaveChangesAsync();

        foreach (var p in game.Participants.Where(p => p.UserId != userId))
        {
            await _notificationService.CreateNotificationAsync(
                p.UserId,
                $"Game '{game.Title}' has been cancelled by the host.",
                NotificationType.GameCancelled
            );
        }

        return true;
    }
}