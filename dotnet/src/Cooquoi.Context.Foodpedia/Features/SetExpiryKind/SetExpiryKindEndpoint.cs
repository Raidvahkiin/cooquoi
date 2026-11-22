using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Persistence;
using Cooquoi.Platform.Authorization;
using Cooquoi.Platform.Concurrency;
using Cooquoi.Platform.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Raidvahkiin.Ddd;
using Wolverine.Http;

namespace Cooquoi.Context.Foodpedia.Features.SetExpiryKind;

public static class SetExpiryKindEndpoint
{
    /// <summary>OpenAPI responses of the contract template.</summary>
    public static void Configure(HttpChain chain) => OperationContract.ReplaceAction(chain);

    /// <summary>Wolverine runs this before <see cref="Post"/>: 404 when the ingredient does not exist.</summary>
    public static async Task<IngredientAggregate> LoadAsync(Guid ingredientId, FoodpediaDbContext db, CancellationToken cancellationToken) =>
        await db.Ingredients.FindAsync([new IngredientId(ingredientId)], cancellationToken)
            ?? throw CommonError.NotFound.ToException(("resource", "Ingredient"), ("id", ingredientId));

    /// <summary>C11: replace-style, so the client proves which version it edited (If-Match); answers the new version as ETag.</summary>
    [WolverinePost("/v1/ingredients/{ingredientId}:set-expiry-kind", OperationId = "setExpiryKind")]
    [Authorize(Policy = CooquoiPolicies.Curator)]
    [EmptyResponse]
    public static void Post(SetExpiryKind request, IngredientAggregate ingredient, HttpContext http)
    {
        IfMatch.Require(http.Request, ingredient);
        IfMatch.SetETagAfterSave(http, ingredient);
        ingredient.SetExpiryKind(ContractEnumNames.Parse<ExpiryKind>(request.ExpiryKind));
    }
}
