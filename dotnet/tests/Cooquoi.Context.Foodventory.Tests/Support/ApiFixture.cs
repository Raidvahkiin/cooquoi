using Alba;
using Cooquoi.Context.Foodventory.Persistence;
using Cooquoi.Testing;

[assembly: AssemblyFixture(typeof(Cooquoi.Context.Foodventory.Tests.Support.ApiFixture))]

namespace Cooquoi.Context.Foodventory.Tests.Support;

/// <summary>The API on a fresh Postgres, migrated at startup (development environment).</summary>
public sealed class ApiFixture : IAsyncLifetime
{
    private readonly TestApi _api = new();

    public IAlbaHost Host => _api.Host;

    public async ValueTask InitializeAsync() => await _api.StartAsync();

    public ValueTask DisposeAsync() => _api.DisposeAsync();

    public FoodventoryDbContext CreateDbContext() => _api.CreateDbContext<FoodventoryDbContext>();
}
