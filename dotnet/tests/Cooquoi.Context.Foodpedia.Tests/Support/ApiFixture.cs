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
    /// Tracked sessions run one at a time (<see cref="TestApi.TrackAsync"/>); untracked tests still run alongside: filter by your own ids.
    /// </summary>
    public async Task<TrackedScenario> TrackedScenario(Action<Scenario> configure)
    {
        IScenarioResult result = null!;
        var session = await _api.TrackAsync(tracking =>
            tracking.ExecuteAndWaitAsync((Func<IMessageContext, Task>)(async _ => result = await Host.Scenario(configure))));
        return new TrackedScenario(result, session);
    }

    /// <summary>Invokes the message and waits until every message it caused is handled (one tracked session at a time).</summary>
    public Task<ITrackedSession> InvokeTracked(object message) => _api.TrackAsync(tracking => tracking.InvokeMessageAndWaitAsync(message));
}
