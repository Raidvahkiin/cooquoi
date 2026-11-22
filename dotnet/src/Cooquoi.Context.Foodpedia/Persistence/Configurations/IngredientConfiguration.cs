using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cooquoi.Context.Foodpedia.Persistence.Configurations;

/// <summary>
/// Names in a child table (queried, F3 by key), conversions as one jsonb column (always read whole),
/// tags as a text[] column (filtered with ANY), shelf life as columns of the root row.
/// </summary>
internal sealed class IngredientConfiguration : IEntityTypeConfiguration<IngredientAggregate>
{
    public const string UniqueCodeIndex = "ux_ingredients_code";

    /// <summary>F4 safety net: one active ingredient per primary name and locale, ignoring case.</summary>
    public const string UniqueActivePrimaryNameIndex = "ux_ingredient_names_active_primary";

    /// <summary>Copy of the owner's status on each name row, kept by <see cref="ActiveNameInterceptor"/>: a partial index cannot read the parent table.</summary>
    public const string IsActiveProperty = "IsActive";

    public void Configure(EntityTypeBuilder<IngredientAggregate> builder)
    {
        builder.ToTable("ingredients");
        builder.HasKey(ingredient => ingredient.Id);
        builder.Property(ingredient => ingredient.Id).HasConversion(id => id.Value, value => new IngredientId(value));
        builder.Property(ingredient => ingredient.Version).IsConcurrencyToken();
        builder.Ignore(ingredient => ingredient.DomainEvents);

        builder.Property(ingredient => ingredient.Code)
            .HasConversion(code => code.Value, value => IngredientCode.Create(value))
            .HasMaxLength(IngredientCode.MaxLength);
        builder.HasIndex(ingredient => ingredient.Code).IsUnique().HasDatabaseName(UniqueCodeIndex);
        builder.Property(ingredient => ingredient.BaseUnit).HasConversion(UnitConversion.Converter).HasMaxLength(20);
        builder.Property(ingredient => ingredient.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(ingredient => ingredient.ExpiryKind).HasConversion<string>().HasMaxLength(20).HasColumnName("expiry_kind");
        builder.Property(ingredient => ingredient.Conversions).HasConversion(ConversionTableJson.Converter).HasColumnType("jsonb");
        builder.PrimitiveCollection(ingredient => ingredient.Tags)
            .HasField("_tags")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .ElementType(tag => tag.HasConversion(TagCodeConversion.Converter));

        builder.OwnsOne(ingredient => ingredient.ShelfLife, shelfLife =>
        {
            shelfLife.Property(s => s.Days).HasColumnName("shelf_life_days");
            shelfLife.Property(s => s.RecommendedStorage).HasColumnName("shelf_life_storage").HasConversion<string>().HasMaxLength(20);
        });

        builder.OwnsMany(ingredient => ingredient.Names, name =>
        {
            name.ToTable("ingredient_names");
            name.WithOwner().HasForeignKey("ingredient_id");
            name.Property(n => n.Locale).HasConversion(LocaleConversion.Converter).HasMaxLength(8);
            name.Property(n => n.Primary).HasColumnName("primary_name").HasMaxLength(LocalizedName.MaxLength);
            name.PrimitiveCollection(n => n.Synonyms);
            name.Property<bool>(IsActiveProperty);
            name.Property<string>("PrimaryNameKey").HasComputedColumnSql("lower(primary_name)", stored: true);
            name.HasIndex(nameof(LocalizedName.Locale), "PrimaryNameKey")
                .IsUnique()
                .HasFilter("is_active")
                .HasDatabaseName(UniqueActivePrimaryNameIndex);
            name.HasKey("ingredient_id", nameof(LocalizedName.Locale));
        });
        builder.Navigation(ingredient => ingredient.Names).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
