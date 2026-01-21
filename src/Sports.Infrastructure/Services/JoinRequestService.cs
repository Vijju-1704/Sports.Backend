using Sports.Application.DTOs.Games;
using Sports.Application.Interfaces;
using Sports.Domain.Entities;
using Sports.Domain.Interfaces;
using Sports.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Sports.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Sports.Infrastructure.Services;

public interface IJoinRequestService
{
    Task<JoinRequestDto> CreateJoinRequestAsync(int gameId, string userId, CreateJoinRequestDto dto);
    Task<IEnumerable<JoinRequestDto>> GetGameJoinRequestsAsync(int gameId);
    Task<IEnumerable<JoinRequestDto>> GetUserJoinRequestsAsync(string userId);
    Task<bool> RespondToJoinRequestAsync(int requestId, string hostUserId, RespondToJoinRequestDto dto);
    Task<bool> CancelJoinRequestAsync(int requestId, string userId);
}

public class JoinRequestService : IJoinRequestService
{
    private readonly IUnitOfWork Uow;
    private readonly UserManager<ApplicationUser> UserManager;
    private readonly INotificationService NotificationService;

    public JoinRequestService(
        IUnitOfWork uow,
        UserManager<ApplicationUser> userManager,
        INotificationService notificationService)
    {
        Uow = uow;
        UserManager = userManager;
        NotificationService = notificationService;
    }

    public async Task<JoinRequestDto> CreateJoinRequestAsync(int gameId, string userId, CreateJoinRequestDto dto)
    {
        // Check if game exists and requires approval
        var game = await Uow.Repository<Game>().GetByIdAsync(gameId);
        if (game == null)
            throw new Exception("Game not found");

        if (!game.RequireApproval)
            throw new Exception("This game does not require approval");

        // Check if user already has a pending request
        var existingRequest = await Uow.Repository<JoinRequest>()
            .GetQueryable()
            .FirstOrDefaultAsync(r => r.GameId == gameId && r.UserId == userId && r.Status == RequestStatus.Pending);

        if (existingRequest != null)
            throw new Exception("You already have a pending request for this game");

        // Check if already a participant
        var isParticipant = await Uow.Repository<GameParticipant>()
            .GetQueryable()
            .AnyAsync(p => p.GameId == gameId && p.UserId == userId);

        if (isParticipant)
            throw new Exception("You are already a participant in this game");

        var request = new JoinRequest
        {
            GameId = gameId,
            UserId = userId,
            Message = dto.Message,
            Status = RequestStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };

        await Uow.Repository<JoinRequest>().AddAsync(request);
        await Uow.SaveChangesAsync();

        // Notify host
        var user = await UserManager.FindByIdAsync(userId);
        await NotificationService.CreateNotificationAsync(
            game.HostUserId,
            $"{user?.FullName ?? "Someone"} requested to join your game '{game.Title}'",
            NotificationType.JoinRequest,
            request.RequestId,
            "JoinRequest"
        );

        return await GetJoinRequestDtoAsync(request);
    }

    public async Task<IEnumerable<JoinRequestDto>> GetGameJoinRequestsAsync(int gameId)
    {
        var requests = await Uow.Repository<JoinRequest>()
            .GetQueryable()
            .Include(r => r.Game)
            .Where(r => r.GameId == gameId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

        var dtos = new List<JoinRequestDto>();
        foreach (var request in requests)
        {
            dtos.Add(await GetJoinRequestDtoAsync(request));
        }

        return dtos;
    }

    public async Task<IEnumerable<JoinRequestDto>> GetUserJoinRequestsAsync(string userId)
    {
        var requests = await Uow.Repository<JoinRequest>()
            .GetQueryable()
            .Include(r => r.Game)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

        var dtos = new List<JoinRequestDto>();
        foreach (var request in requests)
        {
            dtos.Add(await GetJoinRequestDtoAsync(request));
        }

        return dtos;
    }

    public async Task<bool> RespondToJoinRequestAsync(int requestId, string hostUserId, RespondToJoinRequestDto dto)
    {
        var request = await Uow.Repository<JoinRequest>()
            .GetQueryable()
            .Include(r => r.Game)
            .FirstOrDefaultAsync(r => r.RequestId == requestId);

        if (request == null) return false;

        // Verify host
        if (request.Game.HostUserId != hostUserId) return false;

        // Check if already responded
        if (request.Status != RequestStatus.Pending) return false;

        request.Status = dto.Approve ? RequestStatus.Approved : RequestStatus.Rejected;
        request.RespondedAt = DateTime.UtcNow;
        request.ResponseMessage = dto.ResponseMessage;

        await Uow.SaveChangesAsync();

        // If approved, add as participant
        if (dto.Approve)
        {
            // Check capacity
            var participantCount = await Uow.Repository<GameParticipant>()
                .GetQueryable()
                .CountAsync(p => p.GameId == request.GameId);

            if (participantCount >= request.Game.MaxPlayers)
            {
                request.Status = RequestStatus.Rejected;
                request.ResponseMessage = "Game is now full";
                await Uow.SaveChangesAsync();

                await NotificationService.CreateNotificationAsync(
                    request.UserId,
                    $"Your request to join '{request.Game.Title}' was declined - game is full",
                    NotificationType.Alert
                );

                return false;
            }

            var participant = new GameParticipant
            {
                GameId = request.GameId,
                UserId = request.UserId,
                Status = JoinStatus.Confirmed,
                JoinedAt = DateTime.UtcNow
            };

            await Uow.Repository<GameParticipant>().AddAsync(participant);

            // Update game status if now full
            if (participantCount + 1 >= request.Game.MaxPlayers)
            {
                request.Game.Status = GameStatus.Full;
            }

            await Uow.SaveChangesAsync();

            // Notify user - approved
            await NotificationService.CreateNotificationAsync(
                request.UserId,
                $"Your request to join '{request.Game.Title}' was approved!",
                NotificationType.Info
            );
        }
        else
        {
            // Notify user - rejected
            var message = $"Your request to join '{request.Game.Title}' was declined";
            if (!string.IsNullOrEmpty(dto.ResponseMessage))
            {
                message += $": {dto.ResponseMessage}";
            }

            await NotificationService.CreateNotificationAsync(
                request.UserId,
                message,
                NotificationType.Alert
            );
        }

        return true;
    }

    public async Task<bool> CancelJoinRequestAsync(int requestId, string userId)
    {
        var request = await Uow.Repository<JoinRequest>().GetByIdAsync(requestId);
        if (request == null || request.UserId != userId) return false;

        if (request.Status != RequestStatus.Pending) return false;

        Uow.Repository<JoinRequest>().Remove(request);
        await Uow.SaveChangesAsync();

        return true;
    }

    private async Task<JoinRequestDto> GetJoinRequestDtoAsync(JoinRequest request)
    {
        var user = await UserManager.FindByIdAsync(request.UserId);
        var game = request.Game ?? await Uow.Repository<Game>().GetByIdAsync(request.GameId);

        return new JoinRequestDto
        {
            RequestId = request.RequestId,
            GameId = request.GameId,
            GameTitle = game?.Title ?? "Unknown",
            UserId = request.UserId,
            UserName = user?.FullName ?? "Unknown",
            Message = request.Message,
            Status = request.Status.ToString(),
            RequestedAt = request.RequestedAt,
            RespondedAt = request.RespondedAt,
            ResponseMessage = request.ResponseMessage
        };
    }
}