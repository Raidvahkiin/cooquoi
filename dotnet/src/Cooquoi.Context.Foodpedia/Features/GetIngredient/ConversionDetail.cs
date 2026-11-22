using Cooquoi.Platform.OpenApi;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodpedia.Features.GetIngredient;

public sealed record ConversionDetail([property: ContractCodes(typeof(Unit), nameof(Unit.All))] string Unit, decimal UnitAmount, decimal BaseAmount);
