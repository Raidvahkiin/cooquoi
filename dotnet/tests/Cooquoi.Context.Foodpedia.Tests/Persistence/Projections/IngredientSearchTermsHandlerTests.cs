using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Tests.Support;
using Wolverine.Tracking;

namespace Cooquoi.Context.Foodpedia.Tests.Persistence.Projections;

/// <summary>
/// Search behaviour is covered through the search endpoint; here, how the handler is wired. The
/// generated actor line itself runs in every projection (a broken line would fail their compilation).
/// </summary>
public sealed class IngredientSearchTermsHandlerTests(ApiFixture api)
{
    [Fact]
    public void The_handler_starts_by_naming_its_system_actor()
    {
        var chain = api.Host.GetRuntime().Handlers.ChainFor<IngredientCreated>().ShouldNotBeNull();

        chain.Middleware.First().GetType().Name.ShouldBe("EnterSystemActorFrame");
    }
}
