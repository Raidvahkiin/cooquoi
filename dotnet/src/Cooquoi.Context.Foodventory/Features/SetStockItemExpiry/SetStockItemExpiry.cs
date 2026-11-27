namespace Cooquoi.Context.Foodventory.Features.SetStockItemExpiry;

/// <summary>S7: body of <c>POST /v1/stock-items/{stockItemId}:set-expiry</c>; requires <c>If-Match</c>. A null date clears the expiry.</summary>
public sealed record SetStockItemExpiry(DateOnly? Date);
