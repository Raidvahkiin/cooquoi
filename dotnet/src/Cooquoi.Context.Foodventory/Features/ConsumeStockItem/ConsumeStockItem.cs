using System.Text.Json.Serialization;
using Cooquoi.Platform.OpenApi;
using Cooquoi.Platform.Validation;
using FluentValidation;

namespace Cooquoi.Context.Foodventory.Features.ConsumeStockItem;

/// <summary>S4: body of <c>POST /v1/stock-items/{stockItemId}:consume</c>; <paramref name="Unit"/> defaults to the entered unit. Amounts are the domain's (FV4): 422 <c>foodventory.invalidQuantity</c>.</summary>
public sealed record ConsumeStockItem(
    decimal Amount,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull), ContractCodes(typeof(SharedKernel.Unit), nameof(SharedKernel.Unit.All))] string? Unit = null);

public sealed class ConsumeStockItemValidator : AbstractValidator<ConsumeStockItem>
{
    public ConsumeStockItemValidator() =>
        RuleFor(request => request.Unit).Must(unit => unit is null || SharedKernel.Unit.FromCode(unit) is not null).WithErrorCode(ValidationCodes.OneOf);
}
