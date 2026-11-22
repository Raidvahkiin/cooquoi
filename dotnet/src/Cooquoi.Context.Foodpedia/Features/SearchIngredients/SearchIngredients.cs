using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Platform.OpenApi;
using Cooquoi.Platform.Validation;
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
    [FromQuery(Name = "status"), ContractEnum<IngredientStatus>] string? Status,
    [FromQuery(Name = "sort")] string[]? Sort,
    [FromQuery(Name = "page"), DefaultValue(1), Range(1, int.MaxValue)] int? Page,
    [FromQuery(Name = "pageSize"), DefaultValue(SearchIngredients.DefaultPageSize), Range(1, SearchIngredients.MaxPageSize)] int? PageSize)
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}

public sealed class SearchIngredientsValidator : AbstractValidator<SearchIngredients>
{
    private static readonly string[] Statuses = [.. Enum.GetNames<IngredientStatus>().Select(name => name.ToLowerInvariant())];

    public SearchIngredientsValidator()
    {
        RuleFor(query => query.Q).MaximumLength(100);
        RuleFor(query => query.Status).Must(status => status is null || Statuses.Contains(status)).WithErrorCode(ValidationCodes.OneOf);
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, SearchIngredients.MaxPageSize);
    }
}
