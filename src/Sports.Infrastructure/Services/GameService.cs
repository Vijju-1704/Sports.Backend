using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sports.Application.Common;
using Sports.Application.DTOs.Games;
using Sports.Application.Exceptions;
using Sports.Application.Interfaces;
using Sports.Domain.Constants;
using Sports.Domain.Entities;
using Sports.Domain.Enums;
using Sports.Domain.Interfaces;
using Sports.Infrastructure.Identity;

namespace Sports.Infrastructure.Services;

public class GameService : IGameService
{
    private readonly IUnitOfWork Uow;
    private readonly IMapper Mapper;
    private readonly UserManager<ApplicationUser> UserManager;
    private readonly INotificationService NotificationService;
    private readonly IJoinRequestService JoinRequestService;

    public GameService(
        IUnitOfWork uow,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        INotificationService notificationService,
        IJoinRequestService joinRequestService)
    {
        Uow = uow;
        Mapper = mapper;
        UserManager = userManager;
        NotificationService = notificationService;
        JoinRequestService = joinRequestService;
    }

    public async Task<IEnumerable<GameDto>> GetAllGamesAsync(
        string? sport = null,
        DateTime? date = null,
        int? sportId = null,
        string? city = null)
    {
        var query = Uow.Repository<Game>().GetQueryable();

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

        // Use AutoMapper
        var gameDtos = Mapper.Map<List<GameDto>>(games);

        // Set host names (can't be mapped automatically)
        foreach (var dto in gameDtos)
        {
            var host = await UserManager.FindByIdAsync(dto.HostUserId);
            dto.HostName = host?.FullName ?? MessageStrings.Unknown;
        }

        return gameDtos;
    }

    /// <summary>
    /// Get ALL games including completed and cancelled (for admin)
    /// </summary>
    public async Task<IEnumerable<GameDto>> GetAllGamesIncludingPastAsync()
    {
        var games = await Uow.Repository<Game>()
            .GetQueryable()
            .Include(g => g.Participants)
            .Include(g => g.Sport)
            .Include(g => g.Venue)
            .Include(g => g.JoinRequests)
            .OrderByDescending(g => g.DateTime)
            .ToListAsync();

        var gameDtos = Mapper.Map<List<GameDto>>(games);

        foreach (var dto in gameDtos)
        {
            var host = await UserManager.FindByIdAsync(dto.HostUserId);
            dto.HostName = host?.FullName ?? MessageStrings.Unknown;
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
        var query = Uow.Repository<Game>().GetQueryable();

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
        var gameDtos = Mapper.Map<List<GameDto>>(games);

        // Set host names
        foreach (var dto in gameDtos)
        {
            var host = await UserManager.FindByIdAsync(dto.HostUserId);
            dto.HostName = host?.FullName ?? MessageStrings.Unknown;
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
        var game = await Uow.Repository<Game>()
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

        // Use AutoMapper
        var gameDto = Mapper.Map<GameDetailDto>(game);

        // Set host name
        var host = await UserManager.FindByIdAsync(game.HostUserId);
        gameDto.HostName = host?.FullName ?? MessageStrings.Unknown;

        // Set participant names and IsHost
        foreach (var participantDto in gameDto.Participants)
        {
            var user = await UserManager.FindByIdAsync(participantDto.UserId);
            participantDto.UserName = user?.FullName ??MessageStrings.Unknown;
            participantDto.IsHost = participantDto.UserId == game.HostUserId;
        }

        // Get pending requests
        if (game.RequireApproval)
        {
            var requests = await JoinRequestService.GetGameJoinRequestsAsync(id);
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
            throw new ValidationException("MaxPlayers", MessageStrings.MaxPlayersMustBeGreaterOrEqualMinPlayers);
        }

        // Allow games scheduled for current time or later (with small buffer for form submission delay)
        if (createDto.DateTime < DateTime.Now.AddMinutes(-5))
        {
            throw new ValidationException("DateTime", MessageStrings.GameDateInPast);
        }

        // Use AutoMapper
        var game = Mapper.Map<Game>(createDto);
        game.HostUserId = hostUserId;

        await Uow.Repository<Game>().AddAsync(game);
        await Uow.SaveChangesAsync();

        // Host auto-joins
        var participant = new GameParticipant
        {
            GameId = game.GameId,
            UserId = hostUserId,
            Status = JoinStatus.Confirmed,
            JoinedAt = DateTime.UtcNow
        };
        await Uow.Repository<GameParticipant>().AddAsync(participant);
        await Uow.SaveChangesAsync();

        var result = await GetGameByIdAsync(game.GameId);
        return Mapper.Map<GameDto>(result);
    }

    public async Task<bool> UpdateGameStatusAsync(int gameId, string userId, GameStatus status)
    {
        var game = await Uow.Repository<Game>().GetByIdAsync(gameId);
        
        if (game == null)
            throw new NotFoundException("Game", gameId);

        if (game.HostUserId != userId)
            throw new UnauthorizedException(MessageStrings.OnlyHostCanUpdateStatus);

        game.Status = status;
        await Uow.SaveChangesAsync();

        // Notify participants
        var participants = await Uow.Repository<GameParticipant>()
            .FindAsync(p => p.GameId == gameId);

        // Send appropriate notification based on status
        if (status == GameStatus.Completed)
        {
            // Send rating request notifications to all participants (including host)
            foreach (var p in participants)
            {
                await NotificationService.CreateNotificationAsync(
                    p.UserId,MessageStrings.GameCompletedRate(game.Title),
                   
                    NotificationType.RatingRequest,
                    gameId // Pass the game ID so notification can link to rate players page
                );
            }
        }
        else
        {
            // Send regular status update to other participants (not host)
            string statusMessage = status switch
            {
                GameStatus.InProgress => "has started",
                _ => $"status changed to {status}"
            };

            foreach (var p in participants.Where(p => p.UserId != userId))
            {
                await NotificationService.CreateNotificationAsync(
                    p.UserId,
                    $"Game '{game.Title}' {statusMessage}",
                    NotificationType.Info
                );
            }
        }

        return true;
    }

    public async Task<bool> JoinGameAsync(int gameId, string userId)
    {
        var game = await Uow.Repository<Game>()
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

        await Uow.Repository<GameParticipant>().AddAsync(participant);

        if (game.Participants.Count + 1 >= game.MaxPlayers)
        {
            game.Status = GameStatus.Full;
        }

        await Uow.SaveChangesAsync();

        var user = await UserManager.FindByIdAsync(userId);
        await NotificationService.CreateNotificationAsync(
            game.HostUserId,MessageStrings.UserJoinedGame(user?.FullName ?? "Someone", game.Title),
            NotificationType.JoinRequest
        );

        return true;
    }

    public async Task<bool> LeaveGameAsync(int gameId, string userId)
    {
        var game = await Uow.Repository<Game>()
            .GetQueryable()
            .Include(g => g.Participants)
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null)
            throw new NotFoundException("Game", gameId);

        // Host cannot leave their own game
        if (game.HostUserId == userId)
        {
            throw new ValidationException("Host", MessageStrings.HostCannotLeave);
        }

        var participant = game.Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant == null)
            throw new NotFoundException(MessageStrings.NotAParticipantInGame);

        Uow.Repository<GameParticipant>().Remove(participant);

        // If game was full, reopen it
        if (game.Status == GameStatus.Full && game.Participants.Count - 1 < game.MaxPlayers)
        {
            game.Status = GameStatus.Open;
        }

        await Uow.SaveChangesAsync();

        // Notify host
        var user = await UserManager.FindByIdAsync(userId);
        await NotificationService.CreateNotificationAsync(
            game.HostUserId,MessageStrings.UserLeftGame(user?.FullName ?? "Someone", game.Title),
            NotificationType.Info
        );

        return true;
    }

    public async Task<bool> UpdateGameAsync(int gameId, CreateGameDto updateDto, string userId)
    {
        var game = await Uow.Repository<Game>().GetByIdAsync(gameId);
        
        if (game == null)
            throw new NotFoundException("Game", gameId);

        if (game.HostUserId != userId)
            throw new UnauthorizedException(MessageStrings.OnlyHostCanUpdateGame);

        if (updateDto.MaxPlayers < updateDto.MinPlayers)
        {
            throw new ValidationException("MaxPlayers", MessageStrings.MaxPlayersMustBeGreaterOrEqualMinPlayers);
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

        await Uow.SaveChangesAsync();

        var participants = await Uow.Repository<GameParticipant>()
            .FindAsync(p => p.GameId == gameId && p.UserId != userId);

        foreach (var p in participants)
        {
            await NotificationService.CreateNotificationAsync(
                p.UserId,MessageStrings.GameUpdated(game.Title),
                NotificationType.Info
            );
        }

        return true;
    }

    public async Task<bool> CancelGameAsync(int gameId, string userId)
    {
        var game = await Uow.Repository<Game>()
            .GetQueryable()
            .Include(g => g.Participants)
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null)
            throw new NotFoundException("Game", gameId);

        if (game.HostUserId != userId)
            throw new UnauthorizedException(MessageStrings.OnlyHostCanCancelGame);

        game.Status = GameStatus.Cancelled;
        await Uow.SaveChangesAsync();

        foreach (var p in game.Participants.Where(p => p.UserId != userId))
        {
            await NotificationService.CreateNotificationAsync(
                p.UserId,MessageStrings.GameCancelled(game.Title),
                NotificationType.GameCancelled
            );
        }

        return true;
    }

    // Auto-complete old games
    public async Task AutoCompleteGamesAsync()
    {
        var now = DateTime.UtcNow;

        var gamesToComplete = await Uow.Repository<Game>()
            .GetQueryable()
            .Include(g => g.Participants)
            .Where(g => (g.Status == GameStatus.Open ||
                        g.Status == GameStatus.Full ||
                        g.Status == GameStatus.InProgress) &&
                       g.DateTime.AddMinutes(g.DurationMinutes) < now)
            .ToListAsync();

        foreach (var game in gamesToComplete)
        {
            game.Status = GameStatus.Completed;
            Console.WriteLine($"✅ Auto-completed game: {game.Title} (ID: {game.GameId})");
            
            // Send rating notifications to all participants
            foreach (var participant in game.Participants)
            {
                await NotificationService.CreateNotificationAsync(
                    participant.UserId,MessageStrings.GameCompletedRate(game.Title),
                    NotificationType.RatingRequest,
                    game.GameId // Pass the game ID
                );
            }
        }

        if (gamesToComplete.Any())
        {
            await Uow.SaveChangesAsync();
            Console.WriteLine($"✅ Completed {gamesToComplete.Count} games automatically");
        }
    }

    // Helper method to auto-update game statuses
    private async Task UpdateGameStatusesAsync(IEnumerable<Game> games)
    {
        var now = DateTime.Now;
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
            await Uow.SaveChangesAsync();
        }
    }
}