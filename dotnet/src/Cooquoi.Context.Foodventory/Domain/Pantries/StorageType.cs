namespace Cooquoi.Context.Foodventory.Domain.Pantries;

/// <summary>How a Pantry aggregate keeps food; drives the proposed expiry on put-away.</summary>
public enum StorageType
{
    Fridge,
    Freezer,
    Room,
}
