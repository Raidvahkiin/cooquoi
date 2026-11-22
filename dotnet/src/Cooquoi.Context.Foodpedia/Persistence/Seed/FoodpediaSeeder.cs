using System.Text.Json;
using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Domain.Tags;
using Cooquoi.Context.Foodpedia.Persistence.Configurations;
using Cooquoi.Context.Foodpedia.Persistence.Projections;
using Cooquoi.Platform.Auditing;
using Cooquoi.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Context.Foodpedia.Persistence.Seed;

/// <summary>
/// Idempotent seeding by code: adds what is missing, never changes existing rows. Builds the search
/// projection directly, since no outbox runs outside a Wolverine handler.
/// </summary>
internal static class FoodpediaSeeder
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static async Task SeedAsync(FoodpediaDbContext db, CancellationToken cancellationToken)
    {
        SystemActor.Enter("system:foodpedia.seed");
        var seed = await ReadAsync(cancellationToken);
        var existingTags = await db.Tags.Select(tag => tag.Code).ToListAsync(cancellationToken);
        db.Tags.AddRange(seed.Tags.Select(ToTag).Where(tag => !existingTags.Contains(tag.Code)));

        var existingIngredients = await db.Ingredients.Select(ingredient => ingredient.Code).ToListAsync(cancellationToken);
        var added = seed.Ingredients.Select(ToIngredient).Where(ingredient => !existingIngredients.Contains(ingredient.Code)).ToList();
        db.Ingredients.AddRange(added);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var ingredient in added)
        {
            await IngredientSearchTermsHandler.RebuildAsync(db, ingredient.Id, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task<SeedFile> ReadAsync(CancellationToken cancellationToken)
    {
        await using var stream = typeof(FoodpediaSeeder).Assembly.GetManifestResourceStream("foodpedia-seed.json")
            ?? throw new InvalidOperationException("Missing embedded foodpedia-seed.json");
        return await JsonSerializer.DeserializeAsync<SeedFile>(stream, Options, cancellationToken)
            ?? throw new InvalidOperationException("Empty foodpedia-seed.json");
    }

    private static TagAggregate ToTag(SeedFile.Tag seed) =>
        TagAggregate.Create(
            TagCode.Parse(seed.Code),
            [.. seed.Labels.Select(label => new TagLabel(Locale.FromTagOrDefault(label.Key), label.Value))]);

    private static IngredientAggregate ToIngredient(SeedFile.Ingredient seed) =>
        IngredientAggregate.Create(new NewIngredient(
            IngredientId.New(),
            IngredientCode.Create(seed.Code),
            [.. seed.Names.Select(name => new LocalizedName(Locale.FromTagOrDefault(name.Key), name.Value.Primary, name.Value.Synonyms ?? []))],
            UnitConversion.FromCode(seed.BaseUnit),
            [.. seed.Tags.Select(TagCode.Parse)],
            [.. (seed.Conversions ?? []).Select(c => new Conversion(UnitConversion.FromCode(c.Unit), c.UnitAmount, c.BaseAmount))],
            Enum.Parse<ExpiryKind>(seed.ExpiryKind.Replace("-", ""), ignoreCase: true),
            seed.ShelfLife is null ? null : new ShelfLife(seed.ShelfLife.Days, Enum.Parse<Storage>(seed.ShelfLife.RecommendedStorage, ignoreCase: true))));
}
