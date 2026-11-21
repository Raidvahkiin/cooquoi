using Alba;
using Cooquoi.Platform.Persistence;
using Cooquoi.Testing;

[assembly: AssemblyFixture(typeof(Cooquoi.Platform.Tests.Support.ApiFixture))]

namespace Cooquoi.Platform.Tests.Support;

public sealed class ApiFixture : IAsyncLifetime
{
    private readonly TestApi _api = new();

    public IAlbaHost Host => _api.Host;

    public async ValueTask InitializeAsync() => await _api.StartAsync();

    public ValueTask DisposeAsync() => _api.DisposeAsync();

    public PlatformDbContext CreateDbContext() => _api.CreateDbContext<PlatformDbContext>();
}
