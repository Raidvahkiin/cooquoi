using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Domain.Tags;
using Cooquoi.Context.Foodpedia.Persistence;
using Cooquoi.Platform.Concurrency;
using Cooquoi.Platform.OpenApi;
using Cooquoi.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Raidvahkiin.Ddd;
using Wolverine.Http;

namespace Cooquoi.Context.Foodpedia.Features.GetIngredient;

public static class GetIngredientEndpoint
{
    /// <summary>OpenAPI responses of the contract template.</summary>
    public static void Configure(HttpChain chain) => OperationContract.Read(chain);

    /// <summary>Q2: one ingredient, name in the <c>Accept-Language</c> locale (English fallback); <c>ETag</c> = version.</summary>
    [WolverineGet("/v1/ingredients/{ingredientId}", OperationId = "getIngredient")]
    [Authorize]
    public static async Task<IngredientDetail> Get(
        Guid ingredientId, FoodpediaDbContext db, HttpRequest request, HttpResponse response, CancellationToken cancellationToken)
    {
        var id = new IngredientId(ingredientId);
        var locale = Locale.FromTagOrDefault(request.Headers.AcceptLanguage.FirstOrDefault());
        var english = Locale.English;
        var row = await db.Ingredients.AsNoTracking()
            .Where(ingredient => ingredient.Id == id)
            .Select(ingredient => new Row(
                ingredient.Code,
                ingredient.Names.Where(name => name.Locale == locale || name.Locale == english).ToList(),
                ingredient.BaseUnit,
                ingredient.Conversions,
                ingredient.ExpiryKind,
                ingredient.ShelfLife,
                ingredient.Tags,
                ingredient.Status,
                ingredient.Version))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw CommonError.NotFound.ToException(("resource", "Ingredient"), ("id", ingredientId));

        IfMatch.SetETag(response, row.Version);
        return row.ToDetail(id, locale);
    }

    private sealed record Row(
        IngredientCode Code,
        List<LocalizedName> Names,
        Unit BaseUnit,
        ConversionTable Conversions,
        ExpiryKind ExpiryKind,
        ShelfLife? ShelfLife,
        IReadOnlyList<TagCode> Tags,
        IngredientStatus Status,
        long Version)
    {
        public IngredientDetail ToDetail(IngredientId id, Locale locale)
        {
            var name = Names.FirstOrDefault(n => n.Locale == locale) ?? Names.First(n => n.Locale == Locale.English);
            return new IngredientDetail(
                id.Value,
                Code.Value,
                name.Locale.Code,
                name.Primary,
                name.Synonyms,
                BaseUnit.Code,
                [.. Conversions.Items.Select(c => new ConversionDetail(c.Unit.Code, c.UnitAmount, c.BaseAmount))],
                ContractEnumNames.ToCode(ExpiryKind),
                ShelfLife is null ? null : new ShelfLifeDetail(ShelfLife.Days, ShelfLife.RecommendedStorage.ToString().ToLowerInvariant()),
                [.. Tags.Select(tag => tag.Value)],
                Status.ToString().ToLowerInvariant(),
                Version);
        }
    }
}
