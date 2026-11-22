using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Platform.OpenApi;

namespace Cooquoi.Context.Foodpedia.Features.GetIngredient;

public sealed record ShelfLifeDetail(int Days, [property: ContractEnum<Storage>] string RecommendedStorage);
