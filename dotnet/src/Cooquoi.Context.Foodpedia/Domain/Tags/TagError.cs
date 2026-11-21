using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Tags;

public static class TagError
{
    public static readonly DomainError InvalidCode = new(
        "foodpedia.invalid-tag-code", ErrorKind.BusinessRule, "'{code}' is not a tag code '<namespace>:<value>' with a known namespace.");

    public static readonly DomainError InvalidLabel = new(
        "foodpedia.invalid-tag-label", ErrorKind.BusinessRule, "The tag label in locale '{locale}' is empty.");

    public static readonly DomainError EnglishLabelRequired = new(
        "foodpedia.english-label-required", ErrorKind.BusinessRule, "A tag needs an English label.");

    public static readonly DomainError DuplicateLocale = new(
        "foodpedia.duplicate-locale", ErrorKind.BusinessRule, "Only one label per locale is allowed; '{locale}' appears twice.");

    public static readonly DomainError UnknownTags = new(
        "foodpedia.unknown-tags", ErrorKind.BusinessRule, "Unknown or retired tags: {codes}.");
}
