using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sports.Application.Common;
using Sports.Application.DTOs.Games;
using Sports.Application.Exceptions;
using Sports.Application.Interfaces;
using Sports.Domain.Entities;
using Sports.Domain.Enums;
using Sports.Domain.Interfaces;
using Sports.Infrastructure.Identity;

namespace Sports.Infrastructure.Services;

public class GameService : IGameService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;
    private readonly IJoinRequestService _joinRequestService;

    public GameService(
        IUnitOfWork uow,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        INotificationService notificationService,
        IJoinRequestService joinRequestService)
    {
        _uow = uow;
        _mapper = mapper;
        _userManager = userManager;
        _notificationService = notificationService;
        _joinRequestService = joinRequestService;
    }

    public async Task<IEnumerable<GameDto>> GetAllGamesAsync(
        string? sport = null,
        DateTime? date = null,
        int? sportId = null,
        string? city = null)
    {
        var query = _uow.Repository<Game>().GetQueryable();

        // Only show active games
        query = query.Where(g => g.Status == GameStatus.Open ||
                                  g.Status == GameStatus.Full ||
                                  g.Status == GameStatus.InProgress);

        // Text search
        if (!string.IsNullOrEmpty(sport))
        {
            query = query.Where(g => g.Title.Contains(sport) || g.Sport.Name.Contains(sport));
        }

        // Date filter
        if (date.HasValue)
        {
            query = query.Where(g => g.DateTime.Date == date.Value.Date);
        }

        // Sport filter
        if (sportId.HasValue)
        {
            query = query.Where(g => g.SportId == sportId.Value);
        }

        // City filter
        if (!string.IsNullOrEmpty(city))
        {
            query = query.Where(g => g.Venue.City.Contains(city));
        }

        // Only upcoming games
        var today = DateTime.UtcNow.Date;
        query = query.Where(g => g.DateTime.Date >= today);

        var games = await query
            .Include(g => g.Participants)
            .Include(g => g.Sport)
            .Include(g => g.Venue)
            .Include(g => g.JoinRequests)
            .OrderBy(g => g.DateTime)
            .ToListAsync();

        // Auto-update statuses
        await UpdateGameStatusesAsync(games);

        // ✅ Use AutoMapper
        var gameDtos = _mapper.Map<List<GameDto>>(games);

        // Set host names (can't be mapped automatically)
        foreach (var dto in gameDtos)
        {
            var host = await _userManager.FindByIdAsync(dto.HostUserId);
            dto.HostName = host?.FullName ?? "Unknown";
        }

        return gameDtos;
    }

    /// <summary>
    /// Get games with pagination support
    /// </summary>
    public async Task<PaginatedResponse<GameDto>> GetGamesPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 12,
        string? search = null,
        int? sportId = null,
        string? city = null)
    {
        var query = _uow.Repository<Game>().GetQueryable();

        // Only show active games
        query = query.Where(g => g.Status == GameStatus.Open ||
                                  g.Status == GameStatus.Full ||
                                  g.Status == GameStatus.InProgress);

        // Search filter
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(g => g.Title.Contains(search) || g.Sport.Name.Contains(search));
        }

        // Sport filter
        if (sportId.HasValue)
        {
            query = query.Where(g => g.SportId == sportId.Value);
        }

        // City filter
        if (!string.IsNullOrEmpty(city))
        {
            query = query.Where(g => g.Venue.City.Contains(city));
        }

        // Only upcoming games
        var today = DateTime.UtcNow.Date;
        query = query.Where(g => g.DateTime.Date >= today);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination and fetch
        var games = await query
            .Include(g => g.Participants)
            .Include(g => g.Sport)
            .Include(g => g.Venue)
            .Include(g => g.JoinRequests)
            .OrderBy(g => g.DateTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Auto-update statuses
        await UpdateGameStatusesAsync(games);

        // Map to DTOs
        var gameDtos = _mapper.Map<List<GameDto>>(games);

        // Set host names
        foreach (var dto in gameDtos)
        {
            var host = await _userManager.FindByIdAsync(dto.HostUserId);
            dto.HostName = host?.FullName ?? "Unknown";
        }

        // Create paginated response
        return new PaginatedResponse<GameDto>
        {
            Items = gameDtos,
            Pagination = new PaginationMetadata
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < (int)Math.Ceiling(totalCount / (double)pageSize)
            }
        };
    }


    public async Task<GameDetailDto?> GetGameByIdAsync(int id, string? currentUserId = null)
    {
        var game = await _uow.Repository<Game>()
            .GetQueryable()
            .Include(g => g.Sport)
            .Include(g => g.Venue)
            .Include(g => g.Participants)
            .Include(g => g.JoinRequests)
            .FirstOrDefaultAsync(g => g.GameId == id);

        if (game == null)
            throw new NotFoundException("Game", id);

        // Update status
        await UpdateGameStatusesAsync(new[] { game });

        // ✅ Use AutoMapper
        var gameDto = _mapper.Map<GameDetailDto>(game);

        // Set host name
        var host = await _userManager.FindByIdAsync(game.HostUserId);
        gameDto.HostName = host?.FullName ?? "Unknown";

        // Set participant names and IsHost
        foreach (var participantDto in gameDto.Participants)
        {
            var user = await _userManager.FindByIdAsync(participantDto.UserId);
            participantDto.UserName = user?.FullName ?? "Unknown";
            participantDto.IsHost = participantDto.UserId == game.HostUserId;
        }

        // Get pending requests
        if (game.RequireApproval)
        {
            var requests = await _joinRequestService.GetGameJoinRequestsAsync(id);
            gameDto.PendingRequests = requests.Where(r => r.Status == "Pending").ToList();

            if (!string.IsNullOrEmpty(currentUserId))
            {
                gameDto.CurrentUserHasRequest = gameDto.PendingRequests.Any(r => r.UserId == currentUserId);
            }
        }

        return gameDto;
    }

    public async Task<GameDto> CreateGameAsync(CreateGameDto createDto, string hostUserId)
    {
        if (createDto.MaxPlayers < createDto.MinPlayers)
        {
            throw new ValidationException("MaxPlayers", "Max players must be >= Min players.");
        }

        // Be lenient with DateTime - just check it's not significantly in the past
        if (createDto.DateTime.AddHours(-12) < DateTime.Now)
        {
            throw new ValidationException("DateTime", "Game date must be in the future.");
        }

        // ✅ Use AutoMapper
        var game = _mapper.Map<Game>(createDto);
        game.HostUserId = hostUserId;

        await _uow.Repository<Game>().AddAsync(game);
        await _uow.SaveChangesAsync();

        // Host auto-joins
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
        return _mapper.Map<GameDto>(result);
    }

    public async Task<bool> UpdateGameStatusAsync(int gameId, string userId, GameStatus status)
    {
        var game = await _uow.Repository<Game>().GetByIdAsync(gameId);
        
        if (game == null)
            throw new NotFoundException("Game", gameId);

        if (game.HostUserId != userId)
            throw new UnauthorizedException("Only the host can update game status.");

        game.Status = status;
        await _uow.SaveChangesAsync();

        // Notify participants
        var participants = await _uow.Repository<GameParticipant>()
            .FindAsync(p => p.GameId == gameId && p.UserId != userId);

        string statusMessage = status switch
        {
            GameStatus.InProgress => "has started",
            GameStatus.Completed => "has been completed",
            _ => $"status changed to {status}"
        };

        foreach (var p in participants)
        {
            await _notificationService.CreateNotificationAsync(
                p.UserId,
                $"Game '{game.Title}' {statusMessage}",
                NotificationType.Info
            );
        }

        return true;
    }

    public async Task<bool> JoinGameAsync(int gameId, string userId)
    {
        var game = await _uow.Repository<Game>()
            .GetQueryable()
            .Include(g => g.Participants)
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null)
            throw new NotFoundException("Game", gameId);

        if (game.Status == GameStatus.Cancelled)
            throw new GameCancelledException();

        if (game.Status != GameStatus.Open)
            throw new GameFullException();

        if (game.Participants.Any(p => p.UserId == userId))
            throw new AlreadyJoinedException();

        if (game.Participants.Count >= game.MaxPlayers)
            throw new GameFullException();

        if (game.DateTime < DateTime.UtcNow)
            throw new PastGameException();

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

    public async Task<bool> LeaveGameAsync(int gameId, string userId)
    {
        var game = await _uow.Repository<Game>()
            .GetQueryable()
            .Include(g => g.Participants)
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null)
            throw new NotFoundException("Game", gameId);

        // Host cannot leave their own game
        if (game.HostUserId == userId)
        {
            throw new ValidationException("Host", "Host cannot leave their own game. Cancel it instead.");
        }

        var participant = game.Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant == null)
            throw new NotFoundException("You are not a participant in this game.");

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
        
        if (game == null)
            throw new NotFoundException("Game", gameId);

        if (game.HostUserId != userId)
            throw new UnauthorizedException("Only the host can update the game.");

        if (updateDto.MaxPlayers < updateDto.MinPlayers)
        {
            throw new ValidationException("MaxPlayers", "Max players must be >= Min players.");
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

        if (game == null)
            throw new NotFoundException("Game", gameId);

        if (game.HostUserId != userId)
            throw new UnauthorizedException("Only the host can cancel the game.");

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

    // ✅ Auto-complete old games
    public async Task AutoCompleteGamesAsync()
    {
        var now = DateTime.UtcNow;

        var gamesToComplete = await _uow.Repository<Game>()
            .GetQueryable()
            .Where(g => (g.Status == GameStatus.Open ||
                        g.Status == GameStatus.Full ||
                        g.Status == GameStatus.InProgress) &&
                       g.DateTime.AddMinutes(g.DurationMinutes) < now)
            .ToListAsync();

        foreach (var game in gamesToComplete)
        {
            game.Status = GameStatus.Completed;
            Console.WriteLine($"✅ Auto-completed game: {game.Title} (ID: {game.GameId})");
        }

        if (gamesToComplete.Any())
        {
            await _uow.SaveChangesAsync();
            Console.WriteLine($"✅ Completed {gamesToComplete.Count} games automatically");
        }
    }

    // ✅ Helper method to auto-update game statuses
    private async Task UpdateGameStatusesAsync(IEnumerable<Game> games)
    {
        var now = DateTime.UtcNow;
        bool hasChanges = false;

        foreach (var game in games)
        {
            var gameEndTime = game.DateTime.AddMinutes(game.DurationMinutes);

            // Game has ended -> Mark as Completed
            if (now > gameEndTime && game.Status != GameStatus.Completed && game.Status != GameStatus.Cancelled)
            {
                game.Status = GameStatus.Completed;
                hasChanges = true;
                Console.WriteLine($"✅ Auto-completed game: {game.Title} (ID: {game.GameId})");
            }
            // Game is currently happening -> Mark as InProgress
            else if (now >= game.DateTime && now <= gameEndTime && game.Status == GameStatus.Open)
            {
                game.Status = GameStatus.InProgress;
                hasChanges = true;
                Console.WriteLine($"▶️ Auto-started game: {game.Title} (ID: {game.GameId})");
            }
            // Game is full -> Update status
            else if (game.Participants.Count >= game.MaxPlayers && game.Status == GameStatus.Open)
            {
                game.Status = GameStatus.Full;
                hasChanges = true;
            }
            // Game has spots available -> Reopen
            else if (game.Participants.Count < game.MaxPlayers && game.Status == GameStatus.Full)
            {
                game.Status = GameStatus.Open;
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            await _uow.SaveChangesAsync();
        }
    }
}