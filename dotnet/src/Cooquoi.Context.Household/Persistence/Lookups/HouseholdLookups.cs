using Cooquoi.Context.Household.Domain.Users;
using Microsoft.EntityFrameworkCore;
using HouseholdAggregate = Cooquoi.Context.Household.Domain.Households.Household;

namespace Cooquoi.Context.Household.Persistence.Lookups;

public static class HouseholdLookups
{
    public static Task<UserProfile?> FindByIdentityAsync(
        this IQueryable<UserProfile> profiles, IdentityRef identity, CancellationToken cancellationToken) =>
        profiles.FirstOrDefaultAsync(
            profile => profile.Identity.Issuer == identity.Issuer && profile.Identity.Subject == identity.Subject,
            cancellationToken);

    public static IQueryable<HouseholdAggregate> WithMember(this IQueryable<HouseholdAggregate> households, UserId userId) =>
        households.Where(household => household.Memberships.Any(membership => membership.UserId == userId));
}
