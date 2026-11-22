using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Platform.OpenApi;

namespace Cooquoi.Context.Foodpedia.Tests.Domain.Ingredients;

public sealed class ContractEnumNamesTests
{
    [Fact]
    public void Every_member_round_trips_through_its_code()
    {
        Enum.GetValues<ExpiryKind>().ShouldAllBe(kind => ContractEnumNames.Parse<ExpiryKind>(ContractEnumNames.ToCode(kind)) == kind);
        Enum.GetValues<IngredientStatus>().ShouldAllBe(status => ContractEnumNames.Parse<IngredientStatus>(ContractEnumNames.ToCode(status)) == status);
        Enum.GetValues<Storage>().ShouldAllBe(storage => ContractEnumNames.Parse<Storage>(ContractEnumNames.ToCode(storage)) == storage);
    }

    [Fact]
    public void The_codes_are_the_contract_strings()
    {
        new ContractEnumAttribute<ExpiryKind>().Values.ShouldBe(["printed", "estimated", "long-life"]);
        new ContractEnumAttribute<IngredientStatus>().Values.ShouldBe(["active", "retired"]);
        new ContractEnumAttribute<Storage>().Values.ShouldBe(["fridge", "freezer", "room"]);
    }
}
