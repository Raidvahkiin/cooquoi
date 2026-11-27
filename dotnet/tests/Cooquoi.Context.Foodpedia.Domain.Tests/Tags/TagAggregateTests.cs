using Cooquoi.Context.Foodpedia.Domain.Tags.Events;
using Cooquoi.Context.Foodpedia.Domain.Tags;
using Cooquoi.Context.Foodpedia.Domain.Tests.Support;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodpedia.Domain.Tests.Tags;

public sealed class TagAggregateTests
{
    private static readonly TagCode Meat = TagCode.Parse("category:meat");

    [Fact]
    public void Create_keeps_the_tag_active_and_raises_TagCreated()
    {
        var tag = TagAggregate.Create(TagId.New(), Meat, [new TagLabel(Locale.English, "Meat"), new TagLabel(Locale.Vietnamese, "Thịt")]);

        tag.Status.ShouldBe(TagStatus.Active);
        tag.DomainEvents.ShouldHaveSingleItem().ShouldBe(new TagCreatedDomainEvent(tag.Id));
    }

    [Fact]
    public void F2_create_without_an_English_label_fails() =>
        ((Action)(() => TagAggregate.Create(TagId.New(), Meat, [new TagLabel(Locale.French, "Viande")]))).ShouldFailWith(TagError.EnglishLabelRequired);

    [Fact]
    public void F3_create_with_two_labels_in_one_locale_fails() =>
        ((Action)(() => TagAggregate.Create(TagId.New(), Meat, [new TagLabel(Locale.English, "Meat"), new TagLabel(Locale.English, "Meats")])))
            .ShouldFailWith(TagError.DuplicateLocale);
}
