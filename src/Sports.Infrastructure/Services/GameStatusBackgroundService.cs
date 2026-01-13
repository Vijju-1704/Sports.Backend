using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sports.Application.Interfaces;

namespace Sports.Infrastructure.Services;

/// <summary>
/// Background service that automatically updates game statuses
/// Runs every 5 minutes to check for games that should be completed
/// </summary>
public class GameStatusBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GameStatusBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // Run every 5 minutes

    public GameStatusBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<GameStatusBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🎮 Game Status Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateGameStatusesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating game statuses");
            }

            // Wait for the next interval
            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("🎮 Game Status Background Service stopped");
    }

    private async Task UpdateGameStatusesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

        _logger.LogInformation("🔄 Checking for games that need status updates...");

        await gameService.AutoCompleteGamesAsync();
    }
}