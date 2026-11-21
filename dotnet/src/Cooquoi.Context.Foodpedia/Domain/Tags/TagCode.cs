using System.Text.RegularExpressions;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Tags;

/// <summary><c>&lt;namespace&gt;:&lt;value&gt;</c>, e.g. <c>category:vegetables</c>. Unique and immutable (F1).</summary>
public sealed partial record TagCode
{
    private TagCode(TagNamespace @namespace, string value)
    {
        Namespace = @namespace;
        Value = value;
    }

    public TagNamespace Namespace { get; }

    /// <summary>The full code, namespace included.</summary>
    public string Value { get; }

    public bool IsCategory => Namespace == TagNamespace.Category;

    public static TagCode Parse(string code)
    {
        var match = Format().Match(code);
        var known = Enum.TryParse<TagNamespace>(match.Groups["namespace"].Value, ignoreCase: true, out var @namespace);
        Ensure.That(match.Success && known, TagError.InvalidCode, ("code", code));
        return new TagCode(@namespace, code);
    }

    public override string ToString() => Value;

    [GeneratedRegex("^(?<namespace>[a-z]+):[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex Format();
}
