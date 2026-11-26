using Cooquoi.Context.Household.Domain.Households;
using Cooquoi.Context.Household.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Context.Household.PublicApi;

/// <remarks>Public only so Wolverine's generated code can construct it (a non-public type forces service location).</remarks>
public sealed class HouseholdDirectory(HouseholdDbContext db) : IHouseholdDirectory
{
    public async Task<Guid?> FindHouseholdIdAsync(string issuer, string subject, CancellationToken cancellationToken)
    {
        var householdId = await db.UserProfiles.AsNoTracking()
            .Where(profile => profile.Identity.Issuer == issuer && profile.Identity.Subject == subject)
            .Select(profile => (HouseholdId?)profile.DefaultHouseholdId)
            .FirstOrDefaultAsync(cancellationToken);
        return householdId?.Value;
    }
}
