using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Domain.Tags;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodpedia.Tests.Support;

/// <summary>Valid domain inputs; each test changes only what it is about.</summary>
public static class SampleIngredients
{
    public static readonly TagCode Fish = TagCode.Parse("category:fish-seafood");

    public static NewIngredient Salmon() => new(
        IngredientId.New(),
        IngredientCode.Create("salmon"),
        [Name(Locale.English, "Salmon"), Name(Locale.French, "Saumon"), Name(Locale.Vietnamese, "Cá hồi")],
        Unit.Gram,
        [Fish, TagCode.Parse("family:salmon")],
        [new Conversion(Unit.FromCode("fillet")!, 1, 200)],
        new ShelfLife(2, Storage.Fridge));

    public static LocalizedName Name(Locale locale, string primary, params string[] synonyms) => new(locale, primary, synonyms);
}
