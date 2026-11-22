using Alba;
using Alba.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Wolverine.Tracking;

namespace Cooquoi.Testing;

/// <summary>The real API host (Alba) on a throwaway Postgres; JWTs signed by Alba's stub.</summary>
public sealed class TestApi : IAsyncDisposable
{
    public const string Issuer = "https://identity.test/realms/cooquoi";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();

    // Wolverine keeps one active tracked session per host: a second one started meanwhile steals the first one's records.
    private readonly SemaphoreSlim _tracking = new(1, 1);

    public IAlbaHost Host { get; private set; } = null!;

    public async Task StartAsync()
    {
        await _postgres.StartAsync();
        Host = await AlbaHost.For<Program>(
            web =>
            {
                web.UseSetting("ConnectionStrings:cooquoi", _postgres.GetConnectionString());
                web.UseSetting("Authentication:Authority", Issuer);
            },
            new JwtSecurityStub().With("iss", Issuer));
    }

    public async ValueTask DisposeAsync()
    {
        if (Host is not null)
        {
            await Host.DisposeAsync();
        }

        await _postgres.DisposeAsync();
        _tracking.Dispose();
    }

    /// <summary>
    /// Runs a Wolverine tracked session (30 s timeout), one at a time on this host so parallel tests cannot steal its records.
    /// Untracked tests still run alongside: the session sees their messages too, so filter by your own ids.
    /// </summary>
    public async Task<ITrackedSession> TrackAsync(Func<TrackedSessionConfiguration, Task<ITrackedSession>> run)
    {
        await _tracking.WaitAsync();
        try
        {
            return await run(Host.TrackActivity().Timeout(TimeSpan.FromSeconds(30)));
        }
        finally
        {
            _tracking.Release();
        }
    }

    /// <summary>A fresh unit of work on the test database, to arrange or check what was stored.</summary>
    public TContext CreateDbContext<TContext>()
        where TContext : DbContext =>
        Host.Services.CreateScope().ServiceProvider.GetRequiredService<TContext>();
}
