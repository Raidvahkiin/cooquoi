using Cooquoi.Context.Foodpedia.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cooquoi.Context.Foodpedia.Persistence.Configurations;

internal sealed class TagConfiguration : IEntityTypeConfiguration<TagAggregate>
{
    public void Configure(EntityTypeBuilder<TagAggregate> builder)
    {
        builder.ToTable("tags");
        builder.HasKey(tag => tag.Id);
        builder.Property(tag => tag.Id).HasConversion(id => id.Value, value => new TagId(value));
        builder.Property(tag => tag.Version).IsConcurrencyToken();
        builder.Ignore(tag => tag.DomainEvents);
        builder.Property(tag => tag.Code).HasConversion(TagCodeConversion.Converter).HasMaxLength(100);
        builder.HasIndex(tag => tag.Code).IsUnique().HasDatabaseName("ux_tags_code");
        builder.Property(tag => tag.Status).HasConversion<string>().HasMaxLength(20);

        builder.OwnsMany(tag => tag.Labels, label =>
        {
            label.ToTable("tag_labels");
            label.WithOwner().HasForeignKey("tag_id");
            label.Property(l => l.Locale).HasConversion(LocaleConversion.Converter).HasMaxLength(8);
            label.Property(l => l.Text).HasMaxLength(100);
            label.HasKey("tag_id", nameof(TagLabel.Locale));
        });
        builder.Navigation(tag => tag.Labels).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
