using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Domain.Tags;
using Cooquoi.Context.Foodpedia.Persistence;
using Cooquoi.Context.Foodpedia.Persistence.Lookups;
using Cooquoi.Platform.Authorization;
using Cooquoi.Platform.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Wolverine.Http;

namespace Cooquoi.Context.Foodpedia.Features.CreateIngredient;

public static class CreateIngredientEndpoint
{
    /// <summary>OpenAPI responses of the contract template.</summary>
    public static void Configure(HttpChain chain) => OperationContract.Create(chain);

    /// <summary>C1: a curator adds an ingredient; search finds it once the projection ran.</summary>
    [WolverinePost("/v1/ingredients", OperationId = "createIngredient")]
    [Authorize(Policy = CooquoiPolicies.Curator)]
    public static async Task<Created<CreatedIngredient>> Post(CreateIngredient request, FoodpediaDbContext db, CancellationToken cancellationToken)
    {
        var ingredient = IngredientAggregate.Create(request.ToNewIngredient(IngredientId.New()));
        await CheckAgainstCatalogueAsync(db, ingredient, cancellationToken);
        db.Ingredients.Add(ingredient);
        return TypedResults.Created($"/v1/ingredients/{ingredient.Id.Value}", new CreatedIngredient(ingredient.Id.Value));
    }

    private static async Task CheckAgainstCatalogueAsync(FoodpediaDbContext db, IngredientAggregate ingredient, CancellationToken cancellationToken)
    {
        // F1 (unique index ux_ingredients_code is the safety net)
        if (await db.Ingredients.CodeExistsAsync(ingredient.Code, cancellationToken))
        {
            throw IngredientError.CodeTaken.ToException(("code", ingredient.Code.Value));
        }

        // F4
        if (await db.Ingredients.FirstTakenPrimaryNameAsync(ingredient.Names, cancellationToken) is { } taken)
        {
            throw IngredientError.NameTaken.ToException(("locale", taken.Locale.Code), ("name", taken.Primary));
        }

        // F9
        var missing = await db.Tags.MissingActiveAsync(ingredient.Tags, cancellationToken);
        if (missing.Count > 0)
        {
            throw TagError.UnknownTags.ToException(("codes", string.Join(", ", missing)));
        }
    }
}
