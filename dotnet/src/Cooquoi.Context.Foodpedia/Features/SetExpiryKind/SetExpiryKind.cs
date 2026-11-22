using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Platform.OpenApi;
using Cooquoi.Platform.Validation;
using FluentValidation;

namespace Cooquoi.Context.Foodpedia.Features.SetExpiryKind;

/// <summary>C11: body of <c>POST /v1/ingredients/{id}:set-expiry-kind</c>; requires <c>If-Match</c>.</summary>
public sealed record SetExpiryKind([property: ContractEnum<ExpiryKind>] string ExpiryKind);

public sealed class SetExpiryKindValidator : AbstractValidator<SetExpiryKind>
{
    private static readonly string[] ExpiryKinds = [.. new ContractEnumAttribute<ExpiryKind>().Values];

    public SetExpiryKindValidator() =>
        RuleFor(request => request.ExpiryKind).Must(ExpiryKinds.Contains).WithErrorCode(ValidationCodes.OneOf);
}
