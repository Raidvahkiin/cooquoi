using System.Security.Claims;
using Alba;

namespace Cooquoi.Testing;

/// <summary>Who sends a scenario: claims as Keycloak puts them in the access token.</summary>
public static class Callers
{
    public static Scenario AsUser(this Scenario scenario, string subject = "user-1")
    {
        scenario.WithClaim(new Claim("sub", subject));
        return scenario;
    }

    public static Scenario AsCurator(this Scenario scenario, string subject = "curator-1")
    {
        scenario.WithClaim(new Claim("sub", subject));
        scenario.WithClaim(new Claim("realm_access", """{"roles":["default-roles-cooquoi","catalog-curator"]}""", "JSON"));
        return scenario;
    }

    public static Scenario WithIdempotencyKey(this Scenario scenario, Guid? key = null)
    {
        scenario.WithRequestHeader("Idempotency-Key", (key ?? Guid.NewGuid()).ToString());
        return scenario;
    }
}
