using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using ApplicationName.Worker.Infrastructure;
using Npgsql;

namespace ApplicationName.Worker;

[ExcludeFromCodeCoverage]
public class DatabaseInitializerService(
    NpgsqlDataSource dataSource,
    DatabaseReadiness readiness,
    IHostApplicationLifetime lifetime,
    ILogger<DatabaseInitializerService> logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(30);

    private static readonly TimeSpan RetryTimeout = TimeSpan.FromMinutes(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var delay = RetryDelay;

        while (true)
        {
            try
            {
                await DatabaseInitializer.InitializeAsync(dataSource, stoppingToken);

                readiness.MarkReady();
                logger.LogInformation("Database schema initialized.");

                return;
            }
            // database that is still starting is normal during deployment, so wait for it rather
            // than taking the pod down. Readiness stays false until this succeeds.
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                if (stopwatch.Elapsed + delay > RetryTimeout)
                {
                    logger.LogCritical(exception, "Database initialization failed after {Elapsed}.", stopwatch.Elapsed);

                    Environment.ExitCode = 1;
                    lifetime.StopApplication();

                    return;
                }

                logger.LogWarning(exception, "Database initialization failed; retrying in {RetryDelay}.", delay);

                await Task.Delay(delay, stoppingToken);

                delay = delay.Ticks > MaxRetryDelay.Ticks / 2 ? MaxRetryDelay : delay * 2;
            }
        }
    }
}
