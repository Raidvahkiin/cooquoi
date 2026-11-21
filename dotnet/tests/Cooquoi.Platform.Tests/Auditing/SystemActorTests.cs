using Cooquoi.Platform.Auditing;

namespace Cooquoi.Platform.Tests.Auditing;

public sealed class SystemActorTests
{
    [Theory]
    [InlineData(typeof(Cooquoi.Context.Foodpedia.Persistence.Projections.IngredientSearchTermsHandler), "system:foodpedia.ingredient-search-terms")]
    [InlineData(typeof(SystemActorTests), "system:platform.system-actor-tests")]
    public void A_handler_is_named_after_its_context_and_class(Type handler, string actor) =>
        SystemActor.For(handler).ShouldBe(actor);

    [Fact]
    public async Task The_actor_applies_to_the_entered_flow_only()
    {
        await Task.Run(() => SystemActor.Enter("system:platform.job"), TestContext.Current.CancellationToken);

        SystemActor.Current.ShouldBeNull();
    }
}
