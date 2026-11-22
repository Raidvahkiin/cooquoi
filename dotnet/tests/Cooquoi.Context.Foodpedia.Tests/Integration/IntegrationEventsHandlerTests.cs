using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Tests.Support;
using Wolverine.Tracking;
using Published = Cooquoi.Context.Foodpedia.PublicApi.Events;

namespace Cooquoi.Context.Foodpedia.Tests.Integration;

/// <summary>Each domain event other contexts need is published as its integration event.</summary>
public sealed class IntegrationEventsHandlerTests(ApiFixture api)
{
    [Fact]
    public async Task An_expiry_kind_change_is_published_with_its_kebab_code()
    {
        var id = IngredientId.New();

        var session = await api.InvokeTracked(new IngredientExpiryKindChanged(id, ExpiryKind.LongLife));

        PublishedOnce<Published.IngredientExpiryKindChanged>(session)
            .ShouldBe(new Published.IngredientExpiryKindChanged(id.Value, "long-life"));
    }

    [Fact]
    public async Task A_defined_conversion_is_published()
    {
        var id = IngredientId.New();

        var session = await api.InvokeTracked(new ConversionDefined(id, "fillet", 1, 200));

        PublishedOnce<Published.ConversionDefined>(session).ShouldBe(new Published.ConversionDefined(id.Value, "fillet", 1, 200));
    }

    [Fact]
    public async Task A_retirement_is_published()
    {
        var id = IngredientId.New();

        var session = await api.InvokeTracked(new IngredientRetired(id));

        PublishedOnce<Published.IngredientRetired>(session).ShouldBe(new Published.IngredientRetired(id.Value));
    }

    /// <summary>The published event, seen once whether or not another context subscribes (every tracked stage records it).</summary>
    private static T PublishedOnce<T>(ITrackedSession session) =>
        session.FindEnvelopesWithMessageType<T>().Select(envelope => (T)envelope.Message!).Distinct().ShouldHaveSingleItem();
}
