namespace Cooquoi.Context.Foodpedia.Domain.Tags;

public readonly record struct TagId(Guid Value)
{
    public static TagId New() => new(Guid.CreateVersion7());
}
