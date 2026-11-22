using Cooquoi.Context.Foodpedia.Features.CreateIngredient;

namespace Cooquoi.Context.Foodpedia.Tests.Support;

/// <summary>Valid request bodies with unique codes and names, so tests do not collide on the shared database.</summary>
public static class Requests
{
    public static CreateIngredient NewIngredient(string? suffix = null)
    {
        suffix ??= Guid.NewGuid().ToString("N")[..10];
        return new CreateIngredient(
            $"test-{suffix}",
            [new("en", $"Test bream {suffix}"), new("vi", $"Cá tráp {suffix}", [$"Cá vền {suffix}"])],
            "g",
            ["category:fish-seafood", "allergen:fish"],
            "estimated",
            [new("fillet", 1, 150)],
            new(2, "fridge"));
    }
}
