using Cooquoi.Context.Foodpedia.PublicApi.Events;
using Cooquoi.Context.Foodpedia.Domain.Ingredients.Events;
using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Tests.Support;
using Wolverine.Tracking;

namespace Cooquoi.Context.Foodpedia.Tests.Integration;

/// <summary>Each domain event other contexts need is published as its integration event.</summary>
public sealed class IntegrationEventsHandlerTests(ApiFixture api)
{
    [Fact]
    public async Task An_expiry_kind_change_is_published_with_its_kebab_code()
    {
        var id = IngredientId.New();

        var session = await api.InvokeTracked(new IngredientExpiryKindChangedDomainEvent(id, ExpiryKind.LongLife));

        PublishedOnce<IngredientExpiryKindChangedEvent>(session)
            .ShouldBe(new IngredientExpiryKindChangedEvent(id.Value, "longLife"));
    }

    [Fact]
    public async Task A_defined_conversion_is_published()
    {
        var id = IngredientId.New();

        var session = await api.InvokeTracked(new ConversionDefinedDomainEvent(id, "fillet", 1, 200));

        PublishedOnce<ConversionDefinedEvent>(session).ShouldBe(new ConversionDefinedEvent(id.Value, "fillet", 1, 200));
    }

    [Fact]
    public async Task A_retirement_is_published()
    {
        var id = IngredientId.New();

        var session = await api.InvokeTracked(new IngredientRetiredDomainEvent(id));

        PublishedOnce<IngredientRetiredEvent>(session).ShouldBe(new IngredientRetiredEvent(id.Value));
    }

    /// <summary>The published event, seen once whether or not another context subscribes (every tracked stage records it).</summary>
    private static T PublishedOnce<T>(ITrackedSession session) =>
        session.FindEnvelopesWithMessageType<T>().Select(envelope => (T)envelope.Message!).Distinct().ShouldHaveSingleItem();
}
