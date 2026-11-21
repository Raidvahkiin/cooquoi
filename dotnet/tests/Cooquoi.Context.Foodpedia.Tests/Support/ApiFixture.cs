using Alba;
using Cooquoi.Context.Foodpedia.Persistence;
using Cooquoi.Testing;
using Wolverine;
using Wolverine.Tracking;

[assembly: AssemblyFixture(typeof(Cooquoi.Context.Foodpedia.Tests.Support.ApiFixture))]

namespace Cooquoi.Context.Foodpedia.Tests.Support;

/// <summary>The API on a fresh Postgres, Foodpedia seeded at startup (development environment).</summary>
public sealed class ApiFixture : IAsyncLifetime
{
    private readonly TestApi _api = new();

    public IAlbaHost Host => _api.Host;

    public async ValueTask InitializeAsync() => await _api.StartAsync();

    public ValueTask DisposeAsync() => _api.DisposeAsync();

    public FoodpediaDbContext CreateDbContext() => _api.CreateDbContext<FoodpediaDbContext>();

    public Task<IScenarioResult> Scenario(Action<Scenario> configure) => Host.Scenario(configure);

    /// <summary>
    /// Runs the request and waits until every message it caused (outbox, projections) is handled.
    /// The session sees the whole host, tests running in parallel included: filter by your own ids.
    /// </summary>
    public async Task<TrackedScenario> TrackedScenario(Action<Scenario> configure)
    {
        IScenarioResult result = null!;
        var session = await Host.TrackActivity()
            .Timeout(TimeSpan.FromSeconds(30))
            .ExecuteAndWaitAsync((Func<IMessageContext, Task>)(async _ => result = await Host.Scenario(configure)));
        return new TrackedScenario(result, session);
    }
}
