using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks;

namespace Cooquoi.Context.Foodventory.Domain.Tests.IngredientStocks;

public sealed class ExpiryProposerTests
{
    private static readonly DateOnly AddedOn = TestFacts.Today;
    private static readonly Expiry InFiveDays = new(AddedOn.AddDays(5), ExpirySource.Proposed);

    [Fact]
    public void FV8_printed_is_never_proposed() =>
        ExpiryProposer.Propose(null, ExpiryKind.Printed, null, AddedOn, TestFacts.Of(ExpiryKind.Printed)).ShouldBeNull();

    [Fact]
    public void FV8_long_life_is_never_proposed() =>
        ExpiryProposer.Propose(null, ExpiryKind.LongLife, null, AddedOn, TestFacts.Of(ExpiryKind.LongLife)).ShouldBeNull();

    [Theory]
    [InlineData(null)]
    [InlineData(StorageType.Fridge)]
    public void FV8_estimated_with_no_or_the_recommended_storage_gets_the_Foodpedia_shelf_life(StorageType? storage) =>
        ExpiryProposer.Propose(null, ExpiryKind.Estimated, storage, AddedOn, TestFacts.Of()).ShouldBe(InFiveDays);

    [Fact]
    public void FV8_estimated_in_another_storage_gets_none() =>
        ExpiryProposer.Propose(InFiveDays, ExpiryKind.Estimated, StorageType.Freezer, AddedOn, TestFacts.Of()).ShouldBeNull();

    [Fact]
    public void FV8_estimated_without_shelf_life_gets_none() =>
        ExpiryProposer.Propose(null, ExpiryKind.Estimated, null, AddedOn, TestFacts.Of(shelfLifeDays: null)).ShouldBeNull();

    [Fact]
    public void FV8_a_manual_expiry_is_kept_even_in_the_past()
    {
        var manual = new Expiry(AddedOn.AddDays(-3), ExpirySource.Manual);

        ExpiryProposer.Propose(manual, ExpiryKind.Estimated, StorageType.Freezer, AddedOn, TestFacts.Of()).ShouldBe(manual);
    }

    [Fact]
    public void FV8_a_proposal_from_a_past_restock_may_already_be_past()
    {
        var addedOn = AddedOn.AddDays(-10);

        ExpiryProposer.Propose(null, ExpiryKind.Estimated, null, addedOn, TestFacts.Of())!.Date.ShouldBe(addedOn.AddDays(5));
    }
}
