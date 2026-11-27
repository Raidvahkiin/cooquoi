namespace Cooquoi.Context.Foodventory.Features.AdjustStockItem;

/// <summary>S6: body of <c>POST /v1/stock-items/{stockItemId}:adjust</c>; requires <c>If-Match</c>. The amount is in the entered unit (FV4: 422 <c>foodventory.invalidQuantity</c>).</summary>
public sealed record AdjustStockItem(decimal Remaining);
