using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodpedia.PublicApi;

/// <summary>A quantity converted to the ingredient's base unit.</summary>
public sealed record NormalizedQuantity(decimal Amount, Unit Unit);
