using System.Security.Claims;
using Cooquoi.Platform.Authorization;
using Microsoft.AspNetCore.Http;

namespace Cooquoi.Platform.Authentication;

/// <summary>Identity of the caller, as stated by the validated access token.</summary>
public sealed class CurrentUser(IHttpContextAccessor accessor)
{
    private ClaimsPrincipal Principal =>
        accessor.HttpContext?.User ?? throw new InvalidOperationException("No HTTP context for the current user");

    public string Issuer => Required("iss");
    public string Subject => Required("sub");
    public string DisplayName => Optional("name") ?? Optional("preferred_username") ?? Subject;
    public string? Email => Optional("email");
    public string? Locale => Optional("locale");
    public bool IsCurator => Principal.HasRealmRole(RealmRoles.CatalogCurator);

    private string? Optional(string type) => Principal.FindFirst(type)?.Value;

    private string Required(string type) =>
        Optional(type) ?? throw new InvalidOperationException($"Access token has no '{type}' claim");
}
