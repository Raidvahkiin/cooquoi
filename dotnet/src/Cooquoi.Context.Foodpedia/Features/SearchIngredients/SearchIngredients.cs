using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Cooquoi.Context.Foodpedia.Features.SearchIngredients;

/// <summary>
/// Q1: query string of <c>GET /v1/ingredients</c>. <c>tags</c>: all must match; <c>status</c>: curators
/// only, others always see active ingredients. Constructor-bound: Wolverine's [AsParameters] cannot set init-only members.
/// </summary>
public sealed record SearchIngredients(
    [FromQuery(Name = "q")] string? Q,
    [FromQuery(Name = "tags")] string[]? Tags,
    [FromQuery(Name = "status")] string? Status,
    [FromQuery(Name = "sort")] string[]? Sort,
    [FromQuery(Name = "page")] int? Page,
    [FromQuery(Name = "pageSize")] int? PageSize)
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}

public sealed class SearchIngredientsValidator : AbstractValidator<SearchIngredients>
{
    public SearchIngredientsValidator()
    {
        RuleFor(query => query.Q).MaximumLength(100);
        RuleFor(query => query.Status).Must(status => status is null or "active" or "retired").WithErrorCode("validation.one-of");
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, SearchIngredients.MaxPageSize);
    }
}
