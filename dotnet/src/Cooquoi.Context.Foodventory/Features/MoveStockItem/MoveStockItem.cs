using System.Text.Json.Serialization;
using Cooquoi.Context.Foodventory.Persistence.Projections;
using Cooquoi.Platform.OpenApi;
using Cooquoi.Platform.Validation;
using FluentValidation;

namespace Cooquoi.Context.Foodventory.Features.MoveStockItem;

/// <summary>S2: body of <c>POST /v1/stock-items/{stockItemId}:move</c>; without <paramref name="Amount"/> the whole item moves.</summary>
public sealed record MoveStockItem(
    Guid PantryId,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? Amount = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull), ContractCodes(typeof(SharedKernel.Unit), nameof(SharedKernel.Unit.All))] string? Unit = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DateOnly? Expiry = null);

public sealed class MoveStockItemValidator : AbstractValidator<MoveStockItem>
{
    public MoveStockItemValidator()
    {
        RuleFor(request => request.Unit).Must(unit => unit is null || SharedKernel.Unit.FromCode(unit) is not null).WithErrorCode(ValidationCodes.OneOf);
        // amount and unit go together
        RuleFor(request => request.Unit).NotNull().When(request => request.Amount is not null);
        RuleFor(request => request.Amount).NotNull().When(request => request.Unit is not null);
    }
}

/// <summary>The moved item, and the source item when a split left a remainder.</summary>
public sealed record MoveResult(
    StockItemView Moved,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] StockItemView? Remainder = null);
