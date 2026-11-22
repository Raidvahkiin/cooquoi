namespace Cooquoi.Context.Foodpedia.PublicApi;

/// <summary>An expiry date proposed from shelf life, and where to keep the stock (<c>fridge</c> | <c>freezer</c> | <c>room</c>).</summary>
public sealed record ProposedShelfLife(DateOnly ExpiresOn, string RecommendedStorage);
