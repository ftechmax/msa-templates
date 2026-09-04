using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApplicationName.Worker.Infrastructure;

[ExcludeFromCodeCoverage]
public sealed class DatabaseReadiness : IHealthCheck
{
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task WaitUntilReadyAsync(CancellationToken cancellationToken) => _ready.Task.WaitAsync(cancellationToken);

    public void MarkReady() => _ready.TrySetResult();

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_ready.Task.IsCompletedSuccessfully
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Database initialization has not completed"));
}
