using System.Text.Json.Serialization;
using Cooquoi.Context.Foodventory.Persistence.Projections;
using Cooquoi.Platform.OpenApi;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodventory.Features.Restock;

/// <summary>Offer to define a household conversion; no amounts, since Foodpedia has no default (it would have normalized the line).</summary>
public sealed record ConversionProposal(
    Guid IngredientId,
    [property: ContractCodes(typeof(Unit), nameof(Unit.All))] string Unit,
    [property: ContractCodes(typeof(Unit), nameof(Unit.BaseUnits))] string BaseUnit,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? UnitAmount = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? BaseAmount = null);

public sealed record RestockResult(IReadOnlyList<StockItemView> Items, IReadOnlyList<ConversionProposal> ConversionProposals);
