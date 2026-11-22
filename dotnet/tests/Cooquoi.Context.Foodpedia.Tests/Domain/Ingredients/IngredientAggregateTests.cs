using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Domain.Tags;
using Cooquoi.Context.Foodpedia.Tests.Support;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodpedia.Tests.Domain.Ingredients;

public sealed class IngredientAggregateTests
{
    private static IngredientAggregate CreateSalmon() => IngredientAggregate.Create(SampleIngredients.Salmon());

    [Fact]
    public void Create_keeps_the_ingredient_active_and_raises_IngredientCreated()
    {
        var salmon = CreateSalmon();

        salmon.Status.ShouldBe(IngredientStatus.Active);
        salmon.Version.ShouldBe(0);
        salmon.Conversions.Items.ShouldHaveSingleItem().Unit.Code.ShouldBe("fillet");
        salmon.DomainEvents.ShouldHaveSingleItem().ShouldBe(new IngredientCreated(salmon.Id));
    }

    [Fact]
    public void F2_create_without_an_English_name_fails()
    {
        var request = SampleIngredients.Salmon() with { Names = [SampleIngredients.Name(Locale.French, "Saumon")] };

        ((Action)(() => IngredientAggregate.Create(request))).ShouldFailWith(IngredientError.EnglishNameRequired);
    }

    [Fact]
    public void F3_create_with_two_names_in_one_locale_fails()
    {
        var request = SampleIngredients.Salmon() with
        {
            Names = [SampleIngredients.Name(Locale.English, "Salmon"), SampleIngredients.Name(Locale.English, "Atlantic salmon")],
        };

        ((Action)(() => IngredientAggregate.Create(request))).ShouldFailWith(IngredientError.DuplicateLocale)
            .Parameters["locale"].ShouldBe("en");
    }

    [Fact]
    public void Create_with_a_base_unit_other_than_g_ml_or_piece_fails()
    {
        var request = SampleIngredients.Salmon() with { BaseUnit = Unit.Kilogram, Conversions = [] };

        ((Action)(() => IngredientAggregate.Create(request))).ShouldFailWith(IngredientError.InvalidBaseUnit);
    }

    [Fact]
    public void F7_create_with_a_conversion_to_a_universally_convertible_unit_fails()
    {
        var request = SampleIngredients.Salmon() with { Conversions = [new Conversion(Unit.Kilogram, 1, 1000)] };

        ((Action)(() => IngredientAggregate.Create(request))).ShouldFailWith(IngredientError.ConversionNotNeeded);
    }

    [Fact]
    public void F9_create_without_a_category_tag_fails()
    {
        var request = SampleIngredients.Salmon() with { Tags = [TagCode.Parse("family:salmon")] };

        ((Action)(() => IngredientAggregate.Create(request))).ShouldFailWith(IngredientError.CategoryTagRequired);
    }

    [Fact]
    public void DefineConversion_replaces_the_conversion_of_the_same_unit_and_raises_ConversionDefined()
    {
        var salmon = CreateSalmon();
        var fillet = Unit.FromCode("fillet")!;

        salmon.DefineConversion(new Conversion(fillet, 1, 180));

        salmon.Conversions.Items.ShouldHaveSingleItem().ShouldBe(new Conversion(fillet, 1, 180));
        salmon.DomainEvents.Last().ShouldBe(new ConversionDefined(salmon.Id, "fillet", 1, 180));
    }

    [Fact]
    public void F7_DefineConversion_for_the_base_unit_itself_fails_and_changes_nothing()
    {
        var salmon = CreateSalmon();

        ((Action)(() => salmon.DefineConversion(new Conversion(Unit.Gram, 1, 1)))).ShouldFailWith(IngredientError.ConversionNotNeeded);

        salmon.Conversions.Items.Count.ShouldBe(1);
        salmon.DomainEvents.Count.ShouldBe(1);
    }

    [Fact]
    public void F10_a_retired_ingredient_cannot_define_a_conversion()
    {
        var salmon = CreateSalmon();
        salmon.Retire();

        ((Action)(() => salmon.DefineConversion(new Conversion(Unit.Piece, 1, 200)))).ShouldFailWith(IngredientError.IngredientRetired);
    }

    [Fact]
    public void F12_create_carries_the_expiry_kind()
    {
        CreateSalmon().ExpiryKind.ShouldBe(ExpiryKind.Printed);
    }

    [Fact]
    public void C11_SetExpiryKind_changes_the_kind_and_raises_IngredientExpiryKindChanged()
    {
        var salmon = CreateSalmon();

        salmon.SetExpiryKind(ExpiryKind.Estimated);

        salmon.ExpiryKind.ShouldBe(ExpiryKind.Estimated);
        salmon.DomainEvents.Last().ShouldBe(new IngredientExpiryKindChanged(salmon.Id, ExpiryKind.Estimated));
    }

    [Fact]
    public void C11_SetExpiryKind_to_the_same_kind_raises_nothing()
    {
        var salmon = CreateSalmon();

        salmon.SetExpiryKind(ExpiryKind.Printed);

        salmon.DomainEvents.Count.ShouldBe(1);
    }

    [Fact]
    public void F10_a_retired_ingredient_cannot_change_its_expiry_kind()
    {
        var salmon = CreateSalmon();
        salmon.Retire();

        ((Action)(() => salmon.SetExpiryKind(ExpiryKind.LongLife))).ShouldFailWith(IngredientError.IngredientRetired);
    }

    [Fact]
    public void SetSynonyms_replaces_the_synonyms_of_one_locale_and_raises_IngredientNamesChanged()
    {
        var salmon = CreateSalmon();

        salmon.SetSynonyms(Locale.Vietnamese, ["Cá salmon"]);

        var vietnamese = salmon.NameIn(Locale.Vietnamese);
        vietnamese.Primary.ShouldBe("Cá hồi");
        vietnamese.Synonyms.ShouldBe(["Cá salmon"]);
        salmon.DomainEvents.Last().ShouldBe(new IngredientNamesChanged(salmon.Id));
    }

    [Fact]
    public void SetSynonyms_for_a_locale_without_a_name_fails()
    {
        var salmon = IngredientAggregate.Create(SampleIngredients.Salmon() with { Names = [SampleIngredients.Name(Locale.English, "Salmon")] });

        ((Action)(() => salmon.SetSynonyms(Locale.French, ["Saumon"]))).ShouldFailWith(IngredientError.NameMissing);
    }

    [Fact]
    public void NameIn_falls_back_to_English_when_the_locale_has_no_name()
    {
        var salmon = IngredientAggregate.Create(SampleIngredients.Salmon() with { Names = [SampleIngredients.Name(Locale.English, "Salmon")] });

        salmon.NameIn(Locale.Vietnamese).Primary.ShouldBe("Salmon");
    }
}
