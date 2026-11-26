using Cooquoi.Context.Foodventory.Domain.Pantries;
using Cooquoi.Platform.OpenApi;
using Cooquoi.Platform.Validation;
using FluentValidation;

namespace Cooquoi.Context.Foodventory.Features.CreatePantry;

/// <summary>C1: body of <c>POST /v1/pantries</c>. The name's rule (FV1) is the domain's: 422 <c>foodventory.invalidName</c>.</summary>
public sealed record CreatePantry(string Name, [property: ContractEnum<StorageType>] string? Storage = null)
{
    internal NewPantry ToNewPantry(PantryId id, Guid householdId) =>
        new(id, householdId, new PantryName(Name), Storage is null ? null : ContractEnumNames.Parse<StorageType>(Storage));
}

public sealed class CreatePantryValidator : AbstractValidator<CreatePantry>
{
    private static readonly string[] Storages = [.. new ContractEnumAttribute<StorageType>().Values];

    public CreatePantryValidator() =>
        RuleFor(request => request.Storage!).Must(Storages.Contains).WithErrorCode(ValidationCodes.OneOf).When(request => request.Storage is not null);
}
