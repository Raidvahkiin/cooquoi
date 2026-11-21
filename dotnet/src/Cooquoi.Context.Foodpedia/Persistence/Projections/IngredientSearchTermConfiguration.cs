using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cooquoi.Context.Foodpedia.Persistence.Projections;

internal sealed class IngredientSearchTermConfiguration : IEntityTypeConfiguration<IngredientSearchTerm>
{
    public void Configure(EntityTypeBuilder<IngredientSearchTerm> builder)
    {
        builder.ToTable("ingredient_search_terms");
        builder.HasKey(term => term.Id);
        builder.Property(term => term.IngredientId).HasConversion(id => id.Value, value => new IngredientId(value));
        builder.HasIndex(term => term.IngredientId);
        builder.Property(term => term.Locale).HasConversion(LocaleConversion.Converter).HasMaxLength(8);
        builder.Property(term => term.Term).HasMaxLength(100);
        builder.Property(term => term.SearchText)
            .HasComputedColumnSql($"{FoodpediaDbContext.Schema}.{SearchFunctions.UnaccentFunction}(lower(term))", stored: true);
        builder.HasIndex(term => term.SearchText).HasMethod("gin").HasOperators("gin_trgm_ops");
    }
}
