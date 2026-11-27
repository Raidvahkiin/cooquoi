using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.Platform.OpenApi;
using Cooquoi.Platform.Validation;
using FluentValidation;

namespace Cooquoi.Context.Foodventory.Features.DiscardStockItem;

/// <summary>S5: body of <c>POST /v1/stock-items/{stockItemId}:discard</c>.</summary>
public sealed record DiscardStockItem([property: ContractEnum<DiscardReason>] string Reason);

public sealed class DiscardStockItemValidator : AbstractValidator<DiscardStockItem>
{
    public DiscardStockItemValidator() => RuleFor(request => request.Reason).IsContractEnum<DiscardStockItem, DiscardReason>();
}
