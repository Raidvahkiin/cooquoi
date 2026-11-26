using System.Security.Claims;
using Cooquoi.Context.Foodventory.Integration;
using Cooquoi.Context.Foodventory.Tests.Support;
using Cooquoi.Context.Household.PublicApi;
using Cooquoi.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Tests.Integration;

public sealed class CurrentHouseholdTests(ApiFixture api)
{
    [Fact]
    public async Task GetId_returns_the_household_of_a_signed_in_caller()
    {
        var subject = $"subject-{Guid.NewGuid()}";
        await api.Host.Scenario(s =>
        {
            s.Post.Url("/v1/me:sign-in");
            s.WithRequestHeader("Idempotency-Key", Guid.NewGuid().ToString());
            s.WithClaim(new Claim("sub", subject));
            s.StatusCodeShouldBe(204);
        });
        using var scope = ScopeFor(subject);
        var expected = await scope.ServiceProvider.GetRequiredService<IHouseholdDirectory>()
            .FindHouseholdIdAsync(TestApi.Issuer, subject, TestContext.Current.CancellationToken);

        var householdId = await scope.ServiceProvider.GetRequiredService<CurrentHousehold>().GetIdAsync(TestContext.Current.CancellationToken);

        householdId.ShouldBe(expected!.Value);
    }

    [Fact]
    public async Task GetId_throws_not_found_before_the_first_sign_in()
    {
        using var scope = ScopeFor($"subject-{Guid.NewGuid()}");

        var exception = await Should.ThrowAsync<DomainException>(() =>
            scope.ServiceProvider.GetRequiredService<CurrentHousehold>().GetIdAsync(TestContext.Current.CancellationToken));

        exception.Error.ShouldBe(CommonError.NotFound);
    }

    /// <summary>A request scope whose caller is <paramref name="subject"/>, as the token would state it.</summary>
    private IServiceScope ScopeFor(string subject)
    {
        var scope = api.Host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("iss", TestApi.Issuer), new Claim("sub", subject)], "test")),
        };
        return scope;
    }
}
