using Cooquoi.Context.Household.Domain.Households;
using Cooquoi.Context.Household.Domain.Users;
using Cooquoi.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cooquoi.Context.Household.Persistence.Configurations;

internal sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");
        builder.HasKey(profile => profile.Id);
        builder.Property(profile => profile.Id).HasConversion(id => id.Value, value => new UserId(value));
        builder.Property(profile => profile.Version).IsConcurrencyToken();
        builder.Ignore(profile => profile.DomainEvents);

        builder.OwnsOne(profile => profile.Identity, identity =>
        {
            identity.Property(i => i.Issuer).HasColumnName("identity_issuer").HasMaxLength(500);
            identity.Property(i => i.Subject).HasColumnName("identity_subject").HasMaxLength(200);
            identity.HasIndex(i => new { i.Issuer, i.Subject }).IsUnique().HasDatabaseName(UniqueIdentityIndex);
        });

        builder.Property(profile => profile.DisplayName).HasMaxLength(200);
        builder.Property(profile => profile.Email).HasMaxLength(320);
        builder.Property(profile => profile.Locale)
            .HasConversion(locale => locale.Code, code => Locale.FromTagOrDefault(code))
            .HasMaxLength(8);
        builder.Property(profile => profile.DefaultHouseholdId)
            .HasConversion(id => id.Value, value => new HouseholdId(value));
    }

    /// <summary>Safety net for concurrent first sign-ins of the same identity.</summary>
    public const string UniqueIdentityIndex = "ux_user_profiles_identity";
}
