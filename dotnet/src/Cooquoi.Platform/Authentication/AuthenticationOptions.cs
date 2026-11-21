using System.ComponentModel.DataAnnotations;

namespace Cooquoi.Platform.Authentication;

public sealed class CooquoiAuthenticationOptions
{
    public const string Section = "Authentication";

    /// <summary>Keycloak realm URL, e.g. https://auth.cooquoi.com/realms/cooquoi.</summary>
    [Required, Url]
    public string Authority { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = "cooquoi-api";
}
