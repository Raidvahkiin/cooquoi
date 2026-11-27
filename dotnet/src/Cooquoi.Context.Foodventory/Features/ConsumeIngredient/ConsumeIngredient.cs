using System.Text.Json.Serialization;
using Cooquoi.Platform.OpenApi;
using Cooquoi.Platform.Validation;
using FluentValidation;

namespace Cooquoi.Context.Foodventory.Features.ConsumeIngredient;

/// <summary>S3: body of <c>POST /v1/stock/{ingredientId}:consume</c>; <paramref name="PantryId"/> limits it to one Pantry aggregate. Amounts are the domain's (FV4): 422 <c>foodventory.invalidQuantity</c>.</summary>
public sealed record ConsumeIngredient(
    decimal Amount,
    [property: ContractCodes(typeof(SharedKernel.Unit), nameof(SharedKernel.Unit.All))] string Unit,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Guid? PantryId = null);

public sealed class ConsumeIngredientValidator : AbstractValidator<ConsumeIngredient>
{
    public ConsumeIngredientValidator() =>
        RuleFor(request => request.Unit).Must(unit => SharedKernel.Unit.FromCode(unit) is not null).WithErrorCode(ValidationCodes.OneOf);
}
