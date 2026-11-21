using Alba;
using Alba.Security;
using Cooquoi.Context.Household.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(Cooquoi.Context.Household.Tests.Support.ApiFixture))]

namespace Cooquoi.Context.Household.Tests.Support;

/// <summary>The real API host (Alba) on a throwaway Postgres; JWTs signed by Alba's stub.</summary>
public sealed class ApiFixture : IAsyncLifetime
{
    public const string Issuer = "https://identity.test/realms/cooquoi";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();

    public IAlbaHost Host { get; private set; } = null!;

    public async ValueTask InitializeAsync()
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
    }

    /// <summary>A fresh unit of work on the test database, to check what was stored.</summary>
    public HouseholdDbContext CreateDbContext() =>
        Host.Services.CreateScope().ServiceProvider.GetRequiredService<HouseholdDbContext>();
}
