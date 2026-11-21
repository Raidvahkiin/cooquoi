using Alba;
using Cooquoi.Context.Household.Persistence;
using Cooquoi.Testing;

[assembly: AssemblyFixture(typeof(Cooquoi.Context.Household.Tests.Support.ApiFixture))]

namespace Cooquoi.Context.Household.Tests.Support;

public sealed class ApiFixture : IAsyncLifetime
{
    private readonly TestApi _api = new();

    public IAlbaHost Host => _api.Host;

    public async ValueTask InitializeAsync() => await _api.StartAsync();

    public ValueTask DisposeAsync() => _api.DisposeAsync();

    public HouseholdDbContext CreateDbContext() => _api.CreateDbContext<HouseholdDbContext>();
}
