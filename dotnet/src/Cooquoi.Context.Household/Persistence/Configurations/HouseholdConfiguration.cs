using Cooquoi.Context.Household.Domain.Households;
using Cooquoi.Context.Household.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HouseholdAggregate = Cooquoi.Context.Household.Domain.Households.Household;

namespace Cooquoi.Context.Household.Persistence.Configurations;

internal sealed class HouseholdConfiguration : IEntityTypeConfiguration<HouseholdAggregate>
{
    public void Configure(EntityTypeBuilder<HouseholdAggregate> builder)
    {
        builder.ToTable("households");
        builder.HasKey(household => household.Id);
        builder.Property(household => household.Id).HasConversion(id => id.Value, value => new HouseholdId(value));
        builder.Property(household => household.Version).IsConcurrencyToken();
        builder.Property(household => household.Name).HasMaxLength(200);
        builder.Ignore(household => household.DomainEvents);

        builder.OwnsMany(household => household.Memberships, membership =>
        {
            membership.ToTable("memberships");
            membership.WithOwner().HasForeignKey("household_id");
            membership.Property(m => m.UserId).HasConversion(id => id.Value, value => new UserId(value));
            membership.Property(m => m.Role).HasConversion<string>().HasMaxLength(20);
            membership.HasKey("household_id", nameof(Membership.UserId));
            membership.HasIndex(m => m.UserId);
        });
        builder.Navigation(household => household.Memberships).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
