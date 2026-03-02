using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sports.Application.Interfaces;
using Sports.Domain.Constants;

namespace Sports.Infrastructure.Services;


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
        Logger.LogInformation(MessageStrings.GameStatusServiceStarted);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateGameStatusesAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, MessageStrings.ErrorUpdatingGameStatuses);
            }

            // Wait for the next interval
            await Task.Delay(Interval, stoppingToken);
        }

        Logger.LogInformation(MessageStrings.GameStatusServiceStopped);
    }

    private async Task UpdateGameStatusesAsync()
    {
        using var scope = ServiceProvider.CreateScope();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

        Logger.LogInformation(MessageStrings.CheckingGameStatusUpdates);

        await gameService.AutoCompleteGamesAsync();
    }
}