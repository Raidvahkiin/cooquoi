using Cooquoi.Platform.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cooquoi.Platform.Idempotency;

/// <summary>
/// Keys are kept 7 days (long enough for offline queues), then deleted hourly (ADR 0014 §6d). The
/// first run waits one interval (the schema may still be migrating); a failed run is logged and
/// retried at the next tick, never stopping the host.
/// </summary>
public sealed partial class IdempotencyCleanup(IServiceScopeFactory scopes, TimeProvider time, ILogger<IdempotencyCleanup> logger)
    : BackgroundService
{
    public static readonly TimeSpan Retention = TimeSpan.FromDays(7);

    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    public static Task<int> DeleteExpiredAsync(PlatformDbContext db, DateTimeOffset now, CancellationToken cancellationToken) =>
        db.IdempotentRequests.Where(request => request.CreatedAt < now - Retention).ExecuteDeleteAsync(cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval, time);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var deleted = await DeleteExpiredAsync(scope.ServiceProvider.GetRequiredService<PlatformDbContext>(), time.GetUtcNow(), stoppingToken);
                LogDeleted(deleted);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                LogFailed(exception);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Deleted {Count} expired idempotency keys")]
    private partial void LogDeleted(int count);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Idempotency key cleanup failed; retrying at the next interval")]
    private partial void LogFailed(Exception exception);
}
