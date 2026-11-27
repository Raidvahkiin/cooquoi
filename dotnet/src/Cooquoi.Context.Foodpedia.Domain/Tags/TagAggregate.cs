using Cooquoi.Context.Foodpedia.Domain.Tags.Events;
using Cooquoi.SharedKernel;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Tags;

/// <summary>One classification value (category, season, diet, allergen, family) with translated labels.</summary>
public sealed class TagAggregate : AggregateRoot<TagId>
{
    private readonly List<TagLabel> _labels = [];

    private TagAggregate() { }

    public TagCode Code { get; private set; } = null!;
    public IReadOnlyList<TagLabel> Labels => _labels;
    public TagStatus Status { get; private set; }

    public static TagAggregate Create(TagId id, TagCode code, IReadOnlyList<TagLabel> labels)
    {
        // F2
        Ensure.That(labels.Any(label => label.Locale == Locale.English), TagError.EnglishLabelRequired);
        // F3
        var duplicate = labels.GroupBy(label => label.Locale).FirstOrDefault(group => group.Count() > 1);
        Ensure.That(duplicate is null, TagError.DuplicateLocale, ("locale", duplicate?.Key.Code));

        var tag = new TagAggregate { Id = id, Code = code, Status = TagStatus.Active };
        tag._labels.AddRange(labels);
        tag.Raise(new TagCreatedDomainEvent(tag.Id));
        return tag;
    }
}
