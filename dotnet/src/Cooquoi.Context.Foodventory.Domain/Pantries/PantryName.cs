using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Domain.Pantries;

/// <summary>FV1: a pantry name, trimmed, 1 to 60 characters.</summary>
public sealed record PantryName
{
    public const int MaxLength = 60;

    public PantryName(string value)
    {
        var trimmed = value?.Trim() ?? "";
        Ensure.That(trimmed.Length is >= 1 and <= MaxLength, PantryError.InvalidName, ("name", value ?? ""));
        Value = trimmed;
    }

    public string Value { get; }
}
