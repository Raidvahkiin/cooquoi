using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Cooquoi.Context.Foodventory.Domain.Pantries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cooquoi.Context.Foodventory.Persistence.Configurations;

internal sealed class PantryConfiguration : IEntityTypeConfiguration<PantryAggregate>
{
    /// <summary>FV1 safety net: one pantry per household and name, ignoring case and accents.</summary>
    private const string NameKey = "NameKey";

    public const string UniqueNameIndex = "ux_pantries_household_name_key";

    public void Configure(EntityTypeBuilder<PantryAggregate> builder)
    {
        builder.ToTable("pantries");
        builder.HasKey(pantry => pantry.Id);
        builder.Property(pantry => pantry.Id).HasConversion(id => id.Value, value => new PantryId(value));
        builder.Property(pantry => pantry.Version).IsConcurrencyToken();
        builder.Ignore(pantry => pantry.DomainEvents);

        builder.Property(pantry => pantry.Name)
            .HasConversion(name => name.Value, value => new PantryName(value))
            .HasMaxLength(PantryName.MaxLength);
        builder.Property<string>(NameKey).HasComputedColumnSql($"{FoodventoryDbContext.Schema}.{PantryNameKey.UnaccentFunction}(lower(name))", stored: true);
        builder.Property(pantry => pantry.Storage).HasMaxLength(20);
        builder.HasIndex([nameof(PantryAggregate.HouseholdId), NameKey]).IsUnique().HasDatabaseName(UniqueNameIndex);
    }
}
