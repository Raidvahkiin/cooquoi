using Cooquoi.SharedKernel;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Tags;

/// <summary>The translated label of a tag in one locale.</summary>
public sealed record TagLabel
{
    public TagLabel(Locale locale, string text)
    {
        Ensure.That(!string.IsNullOrWhiteSpace(text), TagError.InvalidLabel, ("locale", locale.Code));
        Locale = locale;
        Text = text.Trim();
    }

    public Locale Locale { get; }
    public string Text { get; }
}
