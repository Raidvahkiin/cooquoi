using System.Security.Claims;
using System.Text.Json;

namespace Cooquoi.Platform.Authorization;

/// <summary>Keycloak realm roles, carried in the access token as <c>realm_access.roles</c>.</summary>
public static class RealmRoles
{
    public const string CatalogCurator = "catalog-curator";

    private const string RealmAccessClaim = "realm_access";

    public static bool HasRealmRole(this ClaimsPrincipal principal, string role) =>
        principal.FindAll(RealmAccessClaim).Any(claim => Contains(claim.Value, role));

    private static bool Contains(string realmAccess, string role)
    {
        using var document = JsonDocument.Parse(realmAccess);
        return document.RootElement.TryGetProperty("roles", out var roles)
            && roles.ValueKind == JsonValueKind.Array
            && roles.EnumerateArray().Any(item => item.GetString() == role);
    }
}
