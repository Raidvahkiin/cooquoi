using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Persistence;
using Cooquoi.Platform.Authorization;
using Cooquoi.Platform.Concurrency;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Raidvahkiin.Ddd;
using Wolverine.Http;

namespace Cooquoi.Context.Foodpedia.Features.DefineConversion;

public static class DefineConversionEndpoint
{
    /// <summary>Wolverine runs this before <see cref="Post"/>: 404 when the ingredient does not exist.</summary>
    public static async Task<IngredientAggregate> LoadAsync(Guid ingredientId, FoodpediaDbContext db, CancellationToken cancellationToken) =>
        await db.Ingredients.FindAsync([new IngredientId(ingredientId)], cancellationToken)
            ?? throw CommonError.NotFound.ToException(("resource", "Ingredient"), ("id", ingredientId));

    /// <summary>C5: replace-style, so the client proves which version it edited (If-Match); answers the new version as ETag.</summary>
    [WolverinePost("/v1/ingredients/{ingredientId}:define-conversion", OperationId = "defineConversion")]
    [Authorize(Policy = CooquoiPolicies.Curator)]
    [EmptyResponse]
    public static void Post(DefineConversion request, IngredientAggregate ingredient, HttpContext http)
    {
        IfMatch.Require(http.Request, ingredient);
        IfMatch.SetETagAfterSave(http, ingredient);
        ingredient.DefineConversion(new Conversion(SharedKernel.Unit.FromCode(request.Unit)!, request.UnitAmount, request.BaseAmount));
    }
}
