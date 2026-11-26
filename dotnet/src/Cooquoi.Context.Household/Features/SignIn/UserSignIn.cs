using Cooquoi.Context.Household.Domain.Households;
using Cooquoi.Context.Household.Domain.Users;
using Cooquoi.Context.Household.Persistence;
using Cooquoi.Context.Household.Persistence.Lookups;
using Cooquoi.Platform.Authentication;
using Cooquoi.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Raidvahkiin.Ddd;
using Wolverine.EntityFrameworkCore;

namespace Cooquoi.Context.Household.Features.SignIn;

/// <summary>
/// First sign-in creates the profile and a personal household; later sign-ins refresh
/// name and email from the identity provider (ADR 0015).
/// </summary>
internal static class UserSignIn
{
    public static async Task RecordAsync(SignInRequest request, CancellationToken cancellationToken)
    {
        var identity = new IdentityRef(request.User.Issuer, request.User.Subject);
        var profile = await request.Db.UserProfiles.FindByIdentityAsync(identity, cancellationToken)
            ?? await CreateOnceAsync(request, identity, cancellationToken);
        // Saved by Wolverine's transactional middleware after the endpoint.
        profile.RecordSignIn(new SignInDetails(request.User.DisplayName, request.User.Email, request.Now));
    }

    /// <summary>
    /// Two first requests can race (two tabs). A transaction-scoped advisory lock per identity
    /// serializes them: the second waits, then finds the profile. The unique index
    /// <c>ux_user_profiles_identity</c> stays the safety net. The outbox scrapes <c>HouseholdCreated</c>,
    /// stores it in this transaction, commits it, then sends it.
    /// </summary>
    private static async Task<UserProfileAggregate> CreateOnceAsync(SignInRequest request, IdentityRef identity, CancellationToken cancellationToken)
    {
        var db = request.Db;
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var lockKey = $"household.sign-in:{identity.Issuer}|{identity.Subject}";
        await db.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0))", cancellationToken);

        var profile = await db.UserProfiles.FindByIdentityAsync(identity, cancellationToken);
        if (profile is not null)
        {
            await transaction.CommitAsync(cancellationToken);
            return profile;
        }

        profile = Create(request, identity);
        request.Outbox.Enroll(db);
        await request.Outbox.SaveChangesAndFlushMessagesAsync(cancellationToken);
        // The transactional middleware scrapes again before its own save: the events are already stored.
        foreach (var aggregate in db.ChangeTracker.Entries<IAggregateRoot>())
        {
            aggregate.Entity.ClearDomainEvents();
        }

        return profile;
    }

    private static UserProfileAggregate Create(SignInRequest request, IdentityRef identity)
    {
        var userId = UserId.New();
        var locale = Locale.FromTagOrDefault(request.User.Locale ?? request.AcceptLanguage);
        var household = HouseholdAggregate.CreatePersonal(HouseholdId.New(), userId, locale);
        var profile = UserProfileAggregate.Create(new NewUserProfile(
            userId,
            identity,
            request.User.DisplayName,
            request.User.Email,
            locale,
            household.Id,
            request.Now));

        request.Db.Households.Add(household);
        request.Db.UserProfiles.Add(profile);
        return profile;
    }
}

internal sealed record SignInRequest(HouseholdDbContext Db, IDbContextOutbox Outbox, CurrentUser User, string? AcceptLanguage, DateTimeOffset Now);
