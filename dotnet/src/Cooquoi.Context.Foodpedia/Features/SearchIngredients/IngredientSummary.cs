using Cooquoi.Platform.OpenApi;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodpedia.Features.SearchIngredients;

/// <summary>One search result; the name is in the requested locale, English when missing.</summary>
public sealed record IngredientSummary(Guid Id, string Code, string Name, [property: ContractCodes(typeof(Unit), nameof(Unit.BaseUnits))] string BaseUnit, IReadOnlyList<string> CategoryTags);
