using System.Text.Json.Serialization;
using Cooquoi.Context.Foodventory.Domain.Pantries;
using Cooquoi.Platform.OpenApi;

namespace Cooquoi.Context.Foodventory.Features.ListPantries;

/// <summary>Q1 answer: the household's pantries and how many stock items still wait to be put away.</summary>
public sealed record PantryList(IReadOnlyList<PantrySummary> Pantries, int ToPutAwayCount);

public sealed record PantrySummary(
    Guid Id,
    string Name,
    int ItemCount,
    long Version,
    [property: ContractEnum<StorageType>, JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Storage = null);
