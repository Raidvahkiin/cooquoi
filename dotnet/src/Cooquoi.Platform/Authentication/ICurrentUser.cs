namespace Cooquoi.Platform.Authentication;

/// <summary>Identity of the caller, as stated by the validated access token.</summary>
public interface ICurrentUser
{
    string Issuer { get; }
    string Subject { get; }
    string DisplayName { get; }
    string? Email { get; }
    string? Locale { get; }
}
