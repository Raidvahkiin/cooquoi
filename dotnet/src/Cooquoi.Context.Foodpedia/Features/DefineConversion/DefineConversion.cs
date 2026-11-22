using Cooquoi.Platform.OpenApi;
using Cooquoi.Platform.Validation;
using FluentValidation;

namespace Cooquoi.Context.Foodpedia.Features.DefineConversion;

/// <summary>C5: body of <c>POST /v1/ingredients/{id}:define-conversion</c>; requires <c>If-Match</c>.</summary>
public sealed record DefineConversion([property: ContractCodes(typeof(SharedKernel.Unit), nameof(SharedKernel.Unit.All))] string Unit, decimal UnitAmount, decimal BaseAmount);

public sealed class DefineConversionValidator : AbstractValidator<DefineConversion>
{
    public DefineConversionValidator() =>
        RuleFor(request => request.Unit).Must(code => SharedKernel.Unit.FromCode(code) is not null).WithErrorCode(ValidationCodes.OneOf);
}
