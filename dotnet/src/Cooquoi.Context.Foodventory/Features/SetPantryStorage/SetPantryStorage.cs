using Cooquoi.Context.Foodventory.Domain.Pantries;
using Cooquoi.Platform.OpenApi;
using Cooquoi.Platform.Validation;
using FluentValidation;

namespace Cooquoi.Context.Foodventory.Features.SetPantryStorage;

/// <summary>C3: body of <c>POST /v1/pantries/{pantryId}:set-storage</c>; requires <c>If-Match</c>. A null storage clears it.</summary>
public sealed record SetPantryStorage([property: ContractEnum<StorageType>] string? Storage);

public sealed class SetPantryStorageValidator : AbstractValidator<SetPantryStorage>
{
    private static readonly string[] Storages = [.. new ContractEnumAttribute<StorageType>().Values];

    public SetPantryStorageValidator() =>
        RuleFor(request => request.Storage!).Must(Storages.Contains).WithErrorCode(ValidationCodes.OneOf).When(request => request.Storage is not null);
}
