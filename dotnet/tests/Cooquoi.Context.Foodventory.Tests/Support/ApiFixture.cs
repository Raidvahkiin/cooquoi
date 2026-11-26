using Alba;
using Cooquoi.Context.Foodventory.Persistence;
using Cooquoi.Testing;
using Wolverine;
using Wolverine.Tracking;

[assembly: AssemblyFixture(typeof(Cooquoi.Context.Foodventory.Tests.Support.ApiFixture))]

namespace Cooquoi.Context.Foodventory.Tests.Support;

/// <summary>The API on a fresh Postgres, migrated at startup (development environment).</summary>
public sealed class ApiFixture : IAsyncLifetime
{
    private readonly TestApi _api = new();

    public IAlbaHost Host => _api.Host;

    public async ValueTask InitializeAsync() => await _api.StartAsync();

    public ValueTask DisposeAsync() => _api.DisposeAsync();

    /// <summary>Invokes the message and waits until every message it caused is handled (one tracked session at a time).</summary>
    public Task<ITrackedSession> InvokeTracked(object message) => _api.TrackAsync(tracking => tracking.InvokeMessageAndWaitAsync(message));

    /// <summary>Runs the action and waits until every message it caused is handled (one tracked session at a time).</summary>
    public Task<ITrackedSession> Tracked(Func<Task> action) =>
        _api.TrackAsync(tracking => tracking.ExecuteAndWaitAsync((Func<IMessageContext, Task>)(_ => action())));

    public FoodventoryDbContext CreateDbContext() => _api.CreateDbContext<FoodventoryDbContext>();
}
