namespace Cooquoi.SharedKernel;

/// <summary>Supported languages (ADR 0012). English is the default and fallback.</summary>
public sealed record Locale
{
    public static readonly Locale English = new("en");
    public static readonly Locale French = new("fr");
    public static readonly Locale Vietnamese = new("vi");

    private static readonly Locale[] Supported = [English, French, Vietnamese];

    private Locale(string code) => Code = code;

    public string Code { get; }

    /// <summary>Best supported match for a BCP 47 tag (<c>fr-CA</c> → <c>fr</c>); English otherwise.</summary>
    public static Locale FromTagOrDefault(string? tag)
    {
        var language = tag?.Split('-', '_')[0].ToLowerInvariant();
        return Supported.FirstOrDefault(locale => locale.Code == language) ?? English;
    }

    public override string ToString() => Code;
}
