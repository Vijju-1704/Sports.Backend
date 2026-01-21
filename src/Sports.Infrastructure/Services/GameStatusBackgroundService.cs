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
    private readonly IServiceProvider ServiceProvider;
    private readonly ILogger<GameStatusBackgroundService> Logger;
    private readonly TimeSpan Interval = TimeSpan.FromMinutes(5); // Run every 5 minutes

    public GameStatusBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<GameStatusBackgroundService> logger)
    {
        ServiceProvider = serviceProvider;
        Logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("🎮 Game Status Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateGameStatusesAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "❌ Error updating game statuses");
            }

            // Wait for the next interval
            await Task.Delay(Interval, stoppingToken);
        }

        Logger.LogInformation("🎮 Game Status Background Service stopped");
    }

    private async Task UpdateGameStatusesAsync()
    {
        using var scope = ServiceProvider.CreateScope();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

        Logger.LogInformation("🔄 Checking for games that need status updates...");

        await gameService.AutoCompleteGamesAsync();
    }
}