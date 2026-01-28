using FastEndpoints;
using System.Security.Claims;
using Sports.Application.DTOs.Games;
using Sports.Application.Interfaces;

namespace Sports.Api.Endpoints.Games;

public class CreateGameEndpoint : Endpoint<CreateGameDto, GameDto>
{
    private readonly IGameService _gameService;
    
    public CreateGameEndpoint(IGameService gameService)
    {
        _gameService = gameService;
    }
    
    public override void Configure()
    {
        Post("/api/fast/games");
        Roles("User", "Admin");
    }
    
    public override async Task HandleAsync(CreateGameDto req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        try
        {
            var createdGame = await _gameService.CreateGameAsync(req, userId);
            await SendOkAsync(createdGame, ct);
        }
        catch (Exception ex)
        {
            AddError(ex.Message);
            await SendErrorsAsync(400, ct);
        }
    }
}