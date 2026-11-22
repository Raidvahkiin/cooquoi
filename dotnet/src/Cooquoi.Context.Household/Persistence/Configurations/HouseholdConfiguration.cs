using Cooquoi.Context.Household.Domain.Households;
using Cooquoi.Context.Household.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
            membership.Property(m => m.Id).HasColumnName("user_id").HasConversion(id => id.Value, value => new UserId(value));
            membership.Property(m => m.Role).HasMaxLength(20);
            membership.HasKey("household_id", nameof(MembershipEntity.Id));
            membership.HasIndex(m => m.Id).HasDatabaseName("ix_memberships_user_id");
        });
        builder.Navigation(household => household.Memberships).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
