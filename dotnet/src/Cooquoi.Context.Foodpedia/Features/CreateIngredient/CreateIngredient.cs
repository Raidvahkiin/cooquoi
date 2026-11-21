using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Domain.Tags;
using Cooquoi.SharedKernel;
using FluentValidation;

namespace Cooquoi.Context.Foodpedia.Features.CreateIngredient;

/// <summary>C1: body of <c>POST /v1/ingredients</c>.</summary>
public sealed record CreateIngredient(
    string Code,
    IReadOnlyList<CreateIngredient.Name> Names,
    string BaseUnit,
    IReadOnlyList<string> Tags,
    IReadOnlyList<CreateIngredient.ConversionInput>? Conversions = null,
    CreateIngredient.ShelfLifeInput? ShelfLife = null)
{
    public sealed record Name(string Locale, string Primary, IReadOnlyList<string>? Synonyms = null);

    public sealed record ConversionInput(string Unit, decimal UnitAmount, decimal BaseAmount);

    public sealed record ShelfLifeInput(int Days, string RecommendedStorage);

    /// <summary>Only after validation: locales and units are known.</summary>
    internal NewIngredient ToNewIngredient(IngredientId id) => new(
        id,
        IngredientCode.Create(Code),
        [.. Names.Select(name => new LocalizedName(Locale.FromTagOrDefault(name.Locale), name.Primary, name.Synonyms ?? []))],
        SharedKernel.Unit.FromCode(BaseUnit)!,
        [.. Tags.Select(TagCode.Parse)],
        [.. (Conversions ?? []).Select(c => new Conversion(SharedKernel.Unit.FromCode(c.Unit)!, c.UnitAmount, c.BaseAmount))],
        ShelfLife is null ? null : new Domain.Ingredients.ShelfLife(ShelfLife.Days, Enum.Parse<Storage>(ShelfLife.RecommendedStorage, ignoreCase: true)));
}

/// <summary>Shape only; business rules (F1–F9) are the domain's and the endpoint's pre-checks.</summary>
public sealed class CreateIngredientValidator : AbstractValidator<CreateIngredient>
{
    private static readonly string[] Locales = ["en", "fr", "vi"];
    private static readonly string[] Storages = ["fridge", "freezer", "room"];

    public CreateIngredientValidator()
    {
        RuleFor(request => request.Code).NotEmpty().MaximumLength(IngredientCode.MaxLength);
        RuleFor(request => request.Names).NotEmpty();
        RuleForEach(request => request.Names).ChildRules(name =>
        {
            name.RuleFor(n => n.Locale).Must(Locales.Contains).WithErrorCode("validation.one-of");
            name.RuleFor(n => n.Primary).NotEmpty().MaximumLength(LocalizedName.MaxLength);
        });
        RuleFor(request => request.BaseUnit).Must(IsUnit).WithErrorCode("validation.one-of");
        RuleFor(request => request.Tags).NotEmpty();
        RuleForEach(request => request.Conversions).ChildRules(conversion =>
            conversion.RuleFor(c => c.Unit).Must(IsUnit).WithErrorCode("validation.one-of"));
        RuleFor(request => request.ShelfLife!.RecommendedStorage)
            .Must(Storages.Contains).WithErrorCode("validation.one-of")
            .When(request => request.ShelfLife is not null);
    }

    private static bool IsUnit(string code) => SharedKernel.Unit.FromCode(code) is not null;
}
