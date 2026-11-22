using Cooquoi.Context.Foodpedia.Domain.Ingredients.Events;
using Cooquoi.Context.Foodpedia.Domain.Tags;
using Cooquoi.SharedKernel;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients;

/// <summary>Shared reference knowledge about one food: names, base unit, conversions, shelf life, tags.</summary>
public sealed class IngredientAggregate : AggregateRoot<IngredientId>
{
    private readonly List<LocalizedName> _names = [];
    private readonly List<TagCode> _tags = [];

    private IngredientAggregate() { }

    public IngredientCode Code { get; private set; } = null!;
    public IReadOnlyList<LocalizedName> Names => _names;
    public Unit BaseUnit { get; private set; } = Unit.Gram;
    public ConversionTable Conversions { get; private set; } = ConversionTable.Empty;
    public ShelfLife? ShelfLife { get; private set; }
    public ExpiryKind ExpiryKind { get; private set; }
    public IReadOnlyList<TagCode> Tags => _tags;
    public IngredientStatus Status { get; private set; }

    public static IngredientAggregate Create(NewIngredient request)
    {
        CheckNames(request.Names);
        Ensure.That(request.BaseUnit.IsBaseUnit, IngredientError.InvalidBaseUnit, ("unit", request.BaseUnit.Code));
        // F9 (the tags being active is checked by the use case: it needs the tag aggregates)
        Ensure.That(request.Tags.Any(tag => tag.IsCategory), IngredientError.CategoryTagRequired);
        var conversions = request.Conversions.Aggregate(
            ConversionTable.Empty, (table, conversion) => table.Define(conversion, request.BaseUnit));

        var ingredient = new IngredientAggregate
        {
            Id = request.Id,
            Code = request.Code,
            BaseUnit = request.BaseUnit,
            Conversions = conversions,
            ShelfLife = request.ShelfLife,
            ExpiryKind = request.ExpiryKind,
            Status = IngredientStatus.Active,
        };
        ingredient._names.AddRange(request.Names);
        ingredient._tags.AddRange(request.Tags.Distinct());
        ingredient.Raise(new IngredientCreatedDomainEvent(ingredient.Id));
        return ingredient;
    }

    public LocalizedName NameIn(Locale locale) => LocalizedName.Pick(_names, locale);

    /// <summary>C5: defines the conversion of a unit, replacing the existing one.</summary>
    public void DefineConversion(Conversion conversion)
    {
        EnsureActive();
        Conversions = Conversions.Define(conversion, BaseUnit);
        Raise(new ConversionDefinedDomainEvent(Id, conversion.Unit.Code, conversion.UnitAmount, conversion.BaseAmount));
    }

    /// <summary>C11: changes how the expiry is known; nothing happens when the kind is unchanged.</summary>
    public void SetExpiryKind(ExpiryKind expiryKind)
    {
        EnsureActive();
        if (ExpiryKind == expiryKind)
        {
            return;
        }

        ExpiryKind = expiryKind;
        Raise(new IngredientExpiryKindChangedDomainEvent(Id, expiryKind));
    }

    /// <summary>C3: replaces the synonyms of one locale; the primary name is unchanged.</summary>
    public void SetSynonyms(Locale locale, IReadOnlyList<string> synonyms)
    {
        EnsureActive();
        var index = _names.FindIndex(name => name.Locale == locale);
        Ensure.That(index >= 0, IngredientError.NameMissing, ("locale", locale.Code));
        _names[index] = _names[index].WithSynonyms(synonyms);
        Raise(new IngredientNamesChangedDomainEvent(Id));
    }

    /// <summary>C9: hidden from new uses; existing references stay readable.</summary>
    public void Retire()
    {
        EnsureActive();
        Status = IngredientStatus.Retired;
        Raise(new IngredientRetiredDomainEvent(Id));
    }

    // F10
    private void EnsureActive() =>
        Ensure.That(Status == IngredientStatus.Active, IngredientError.IngredientRetired, ("code", Code.Value));

    private static void CheckNames(IReadOnlyList<LocalizedName> names)
    {
        // F2
        Ensure.That(names.Any(name => name.Locale == Locale.English), IngredientError.EnglishNameRequired);
        // F3
        var duplicate = names.GroupBy(name => name.Locale).FirstOrDefault(group => group.Count() > 1);
        Ensure.That(duplicate is null, IngredientError.DuplicateLocale, ("locale", duplicate?.Key.Code));
    }
}
