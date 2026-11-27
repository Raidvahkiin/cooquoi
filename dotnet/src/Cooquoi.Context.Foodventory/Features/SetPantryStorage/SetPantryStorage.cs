using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Cooquoi.Platform.OpenApi;
using Cooquoi.Platform.Validation;
using FluentValidation;

namespace Cooquoi.Context.Foodventory.Features.SetPantryStorage;

/// <summary>C3: body of <c>POST /v1/pantries/{pantryId}:set-storage</c>; requires <c>If-Match</c>. A null storage clears it.</summary>
public sealed record SetPantryStorage([property: ContractEnum<StorageType>] string? Storage);

public sealed class SetPantryStorageValidator : AbstractValidator<SetPantryStorage>
{
    public SetPantryStorageValidator() =>
        RuleFor(request => request.Storage).IsContractEnum<SetPantryStorage, StorageType>().When(request => request.Storage is not null);
}
