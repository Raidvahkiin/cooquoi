using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Domain.Tags;
using Cooquoi.Context.Foodpedia.Persistence;
using Cooquoi.Platform.Authentication;
using Cooquoi.Platform.OpenApi;
using Cooquoi.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Raidvahkiin.Ddd.EntityFrameworkCore.Paging;
using Wolverine.Http;

namespace Cooquoi.Context.Foodpedia.Features.SearchIngredients;

public static class SearchIngredientsEndpoint
{
    /// <summary>OpenAPI responses of the contract template.</summary>
    public static void Configure(HttpChain chain) => OperationContract.List(chain);

    /// <summary>Q1: search ingredients across every locale; retired ones are visible to curators only.</summary>
    [WolverineGet("/v1/ingredients", OperationId = "searchIngredients")]
    [Authorize]
    public static async Task<Paged<IngredientSummary>> Get(
        [AsParameters] SearchIngredients query,
        FoodpediaDbContext db,
        CurrentUser user,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        var text = IngredientSearch.NormalizeText(query.Q);
        var criteria = new SearchCriteria(
            text,
            [.. (query.Tags ?? []).Select(TagCode.Parse)],
            user.IsCurator && query.Status is not null ? Enum.Parse<IngredientStatus>(query.Status, ignoreCase: true) : IngredientStatus.Active,
            Locale.FromTagOrDefault(request.Headers.AcceptLanguage.FirstOrDefault()));
        var rows = IngredientSearch.Sorts.Apply(
            IngredientSearch.Rows(db, criteria),
            query.Sort is { Length: > 0 } sort ? sort : IngredientSearch.DefaultSort(text),
            row => row.Id);
        var page = await rows.ToPageAsync(new PageRequest(query.Page ?? 1, query.PageSize ?? SearchIngredients.DefaultPageSize), cancellationToken);
        return page.Map(row => row.ToSummary());
    }
}
