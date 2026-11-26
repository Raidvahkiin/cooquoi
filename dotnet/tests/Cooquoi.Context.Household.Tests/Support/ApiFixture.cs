using Alba;
using Cooquoi.Context.Household.Persistence;
using Cooquoi.Context.Household.PublicApi;
using Cooquoi.Testing;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Wolverine.Tracking;

[assembly: AssemblyFixture(typeof(Cooquoi.Context.Household.Tests.Support.ApiFixture))]

namespace Cooquoi.Context.Household.Tests.Support;

public sealed class ApiFixture : IAsyncLifetime
{
    private readonly TestApi _api = new();

    public IAlbaHost Host => _api.Host;

    public async ValueTask InitializeAsync() => await _api.StartAsync();

    public ValueTask DisposeAsync() => _api.DisposeAsync();

    public HouseholdDbContext CreateDbContext() => _api.CreateDbContext<HouseholdDbContext>();

    /// <summary>The published query as another context resolves it.</summary>
    public IHouseholdDirectory CreateDirectory() =>
        Host.Services.CreateScope().ServiceProvider.GetRequiredService<IHouseholdDirectory>();

    /// <summary>Runs the request and waits until every message it caused is handled (one tracked session at a time).</summary>
    public Task<ITrackedSession> TrackedScenario(Action<Scenario> configure) =>
        _api.TrackAsync(tracking =>
            tracking.ExecuteAndWaitAsync((Func<IMessageContext, Task>)(async _ => await Host.Scenario(configure))));
}
