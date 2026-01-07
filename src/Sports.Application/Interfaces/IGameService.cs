using Sports.Application.DTOs.Games;

namespace Sports.Application.Interfaces;

public interface IGameService
{
    Task<IEnumerable<GameDto>> GetAllGamesAsync(
        string? sport = null,
        DateTime? date = null,
        int? sportId = null,
        string? city = null);

    Task<GameDetailDto?> GetGameByIdAsync(int id);
    Task<GameDto> CreateGameAsync(CreateGameDto createDto, string hostUserId);
    Task<bool> UpdateGameAsync(int gameId, CreateGameDto updateDto, string userId);
    Task<bool> JoinGameAsync(int gameId, string userId);
    Task<bool> LeaveGameAsync(int gameId, string userId); // ✅ NEW
    Task<bool> CancelGameAsync(int gameId, string userId);
}