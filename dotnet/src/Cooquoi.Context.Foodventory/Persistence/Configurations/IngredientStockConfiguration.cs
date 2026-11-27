using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cooquoi.Context.Foodventory.Persistence.Configurations;

/// <summary>Items in their own table (queried by pantry and expiry), owned so a change to an item bumps the stock's version.</summary>
internal sealed class IngredientStockConfiguration : IEntityTypeConfiguration<IngredientStockAggregate>
{
    /// <summary>One stock per household and ingredient: the safety net of the use case's find-or-create.</summary>
    public const string UniqueIngredientIndex = "ux_ingredient_stocks_household_ingredient";

    public const string HouseholdIdProperty = "HouseholdId";
    public const string IngredientIdProperty = "IngredientId";

    private static readonly ValueConverter<Unit, string> UnitConverter = new(unit => unit.Code, code => Unit.FromCode(code)!);

    private static readonly ValueConverter<PantryId, Guid> PantryIdConverter = new(id => id.Value, value => new PantryId(value));

    public void Configure(EntityTypeBuilder<IngredientStockAggregate> builder)
    {
        builder.ToTable("ingredient_stocks");
        builder.HasKey(stock => stock.Id);
        builder.Property(stock => stock.Id).HasConversion(id => id.Value, value => new IngredientStockId(value));
        builder.Property(stock => stock.Version).IsConcurrencyToken();
        builder.Ignore(stock => stock.DomainEvents);
        builder.Ignore(stock => stock.Total);

        builder.Property(stock => stock.BaseUnit).HasConversion(UnitConverter).HasMaxLength(20);
        builder.Property(stock => stock.ExpiryKind).HasMaxLength(20);
        builder.HasIndex(stock => new { stock.HouseholdId, stock.IngredientId }).IsUnique().HasDatabaseName(UniqueIngredientIndex);

        builder.OwnsMany(stock => stock.Items, item =>
        {
            item.ToTable("stock_items");
            item.WithOwner().HasForeignKey("ingredient_stock_id");
            item.HasKey(entity => entity.Id);
            item.Property(entity => entity.Id).HasConversion(id => id.Value, value => new StockItemId(value)).ValueGeneratedNever();
            item.Property(entity => entity.PantryId).HasConversion(PantryIdConverter);
            item.Property(entity => entity.Source).HasMaxLength(20);
            item.Property(entity => entity.Remaining).HasColumnName("remaining_amount").HasPrecision(18, 3);
            item.Property(entity => entity.Normalized).HasColumnName("normalized_amount").HasPrecision(18, 6);

            // copies of the stock's household and ingredient, kept by StockItemOwnerInterceptor, for the read-side indexes
            item.Property<Guid>(HouseholdIdProperty);
            item.Property<Guid>(IngredientIdProperty);

            item.OwnsOne(entity => entity.Entered, entered =>
            {
                entered.Property(quantity => quantity.Amount).HasColumnName("entered_amount").HasPrecision(18, 3);
                entered.Property(quantity => quantity.Unit).HasColumnName("entered_unit").HasConversion(UnitConverter).HasMaxLength(20);
            });
            item.OwnsOne(entity => entity.Expiry, expiry =>
            {
                expiry.Property(value => value.Date).HasColumnName("expires_on");
                expiry.Property(value => value.Source).HasColumnName("expiry_source").HasMaxLength(20);
            });

            // the expiry index (household, expires_on) is raw SQL in the migration: owned properties cannot be indexed from here
            item.HasIndex(HouseholdIdProperty, nameof(StockItemEntity.PantryId)).HasDatabaseName("ix_stock_items_household_pantry");
        });
        builder.Navigation(stock => stock.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
