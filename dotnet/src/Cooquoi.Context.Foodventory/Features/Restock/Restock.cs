using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.Platform.OpenApi;
using Cooquoi.Platform.Validation;
using FluentValidation;

namespace Cooquoi.Context.Foodventory.Features.Restock;

/// <summary>S1: body of <c>POST /v1/stock:restock</c>. Amounts are the domain's (FV4): 422 <c>foodventory.invalidQuantity</c>.</summary>
public sealed record Restock([property: MinLength(1)] IReadOnlyList<Restock.Line> Lines)
{
    public const int MaxLines = 50;

    public sealed record Line(
        Guid IngredientId,
        decimal Amount,
        [property: ContractCodes(typeof(SharedKernel.Unit), nameof(SharedKernel.Unit.All))] string Unit,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DateOnly? Expiry = null)
    {
        /// <summary>Only after validation: the unit is known.</summary>
        internal NewStockLine ToNewStockLine() =>
            new(StockItemId.New(), new Quantity(Amount, SharedKernel.Unit.FromCode(Unit)!), Expiry, StockSourceType.Manual);
    }
}

public sealed class RestockValidator : AbstractValidator<Restock>
{
    public RestockValidator()
    {
        RuleFor(request => request.Lines).NotEmpty();
        RuleFor(request => request.Lines).Must(lines => lines.Count <= Restock.MaxLines).WithErrorCode("validation.max");
        RuleForEach(request => request.Lines).ChildRules(line =>
            line.RuleFor(l => l.Unit).Must(unit => SharedKernel.Unit.FromCode(unit) is not null).WithErrorCode(ValidationCodes.OneOf));
    }
}
