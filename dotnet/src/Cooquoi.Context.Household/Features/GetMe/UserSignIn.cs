using Cooquoi.Context.Household.Domain.Households;
using Cooquoi.Context.Household.Domain.Users;
using Cooquoi.Context.Household.Persistence;
using Cooquoi.Context.Household.Persistence.Lookups;
using Cooquoi.Platform.Authentication;
using Cooquoi.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using HouseholdAggregate = Cooquoi.Context.Household.Domain.Households.Household;

namespace Cooquoi.Context.Household.Features.GetMe;

/// <summary>
/// First sign-in creates the profile and a personal household; later sign-ins refresh
/// name and email from the identity provider (ADR 0015).
/// </summary>
internal static class UserSignIn
{
    public static async Task<UserProfile> RecordAsync(SignInRequest request, CancellationToken cancellationToken)
    {
        var identity = new IdentityRef(request.User.Issuer, request.User.Subject);
        var existing = await request.Db.UserProfiles.FindByIdentityAsync(identity, cancellationToken);
        if (existing is not null)
        {
            existing.RecordSignIn(new SignInDetails(request.User.DisplayName, request.User.Email, request.Now));
            await request.Db.SaveChangesAsync(cancellationToken);
            return existing;
        }

        try
        {
            return await CreateAsync(request, identity, cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateIdentity(exception))
        {
            // Two first requests raced (e.g. two tabs): the other one created the profile.
            request.Db.ChangeTracker.Clear();
            return await request.Db.UserProfiles.FindByIdentityAsync(identity, cancellationToken)
                ?? throw new InvalidOperationException("Profile vanished after a duplicate sign-in", exception);
        }
    }

    private static async Task<UserProfile> CreateAsync(SignInRequest request, IdentityRef identity, CancellationToken cancellationToken)
    {
        var userId = UserId.New();
        var household = HouseholdAggregate.CreatePersonal(HouseholdId.New(), userId);
        var profile = UserProfile.Create(new NewUserProfile(
            userId,
            identity,
            request.User.DisplayName,
            request.User.Email,
            Locale.FromTagOrDefault(request.User.Locale ?? request.AcceptLanguage),
            household.Id,
            request.Now));

        request.Db.Households.Add(household);
        request.Db.UserProfiles.Add(profile);
        await request.Db.SaveChangesAsync(cancellationToken);
        return profile;
    }

    private static bool IsDuplicateIdentity(DbUpdateException exception) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: Persistence.Configurations.UserProfileConfiguration.UniqueIdentityIndex,
        };
}

internal sealed record SignInRequest(HouseholdDbContext Db, ICurrentUser User, string? AcceptLanguage, DateTimeOffset Now);
