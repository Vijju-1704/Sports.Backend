using Sports.Application.Common;
using Sports.Application.DTOs.Games;
using Sports.Domain.Enums;

namespace Sports.Application.Interfaces;

public interface IGameService
{
    /// <summary>
    /// Get all games with optional filters (non-paginated)
    /// </summary>
    Task<IEnumerable<GameDto>> GetAllGamesAsync(
        string? sport = null,
        DateTime? date = null,
        int? sportId = null,
        string? city = null);

    /// <summary>
    /// Get ALL games including completed and cancelled (for admin)
    /// </summary>
    Task<IEnumerable<GameDto>> GetAllGamesIncludingPastAsync();

    /// <summary>
    /// Get games with pagination
    /// </summary>
    Task<PaginatedResponse<GameDto>> GetGamesPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 12,
        string? search = null,
        int? sportId = null,
        string? city = null);

    Task<GameDetailDto?> GetGameByIdAsync(int id, string? currentUserId = null);
    Task<GameDto> CreateGameAsync(CreateGameDto createDto, string hostUserId);
    Task<bool> UpdateGameAsync(int gameId, CreateGameDto updateDto, string userId);
    Task<bool> JoinGameAsync(int gameId, string userId);
    Task<bool> LeaveGameAsync(int gameId, string userId);
    Task<bool> CancelGameAsync(int gameId, string userId);
    Task<bool> UpdateGameStatusAsync(int gameId, string userId, GameStatus status);
    Task AutoCompleteGamesAsync();
}