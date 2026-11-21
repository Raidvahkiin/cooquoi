using Cooquoi.Platform.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Platform.Idempotency;

/// <summary>Claim, take over, complete and release keys; each statement is atomic on its own.</summary>
internal static class IdempotencyStore
{
    /// <summary>Longer than any request: a claim this old without a commit is a crashed request.</summary>
    public static readonly TimeSpan Lease = TimeSpan.FromMinutes(2);

    public static async Task<bool> TryClaimAsync(this PlatformDbContext db, IdempotentRequest claim, CancellationToken cancellationToken) =>
        await db.Database.ExecuteSqlAsync(
            $"""
            INSERT INTO platform.idempotent_requests (subject, key, request_hash, state, created_at, claimed_at)
            VALUES ({claim.Subject}, {claim.Key}, {claim.RequestHash}, {nameof(IdempotencyState.Claimed)}, {claim.CreatedAt}, {claim.ClaimedAt})
            ON CONFLICT DO NOTHING
            """,
            cancellationToken) == 1;

    /// <summary>The same request, claimed but never committed, past its lease: its first run died.</summary>
    public static async Task<bool> TryTakeOverAsync(this PlatformDbContext db, IdempotentRequest claim, CancellationToken cancellationToken) =>
        await db.IdempotentRequests
            .Where(r => r.Subject == claim.Subject && r.Key == claim.Key && r.RequestHash == claim.RequestHash
                && r.State == IdempotencyState.Claimed && r.ClaimedAt < claim.ClaimedAt - Lease)
            .ExecuteUpdateAsync(set => set.SetProperty(r => r.ClaimedAt, claim.ClaimedAt), cancellationToken) == 1;

    public static Task<IdempotentRequest?> FindAsync(this PlatformDbContext db, IdempotentRequest claim, CancellationToken cancellationToken) =>
        db.IdempotentRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Subject == claim.Subject && r.Key == claim.Key, cancellationToken);

    public static Task CompleteAsync(this PlatformDbContext db, IdempotentRequest response) =>
        db.IdempotentRequests
            .Where(r => r.Subject == response.Subject && r.Key == response.Key)
            .ExecuteUpdateAsync(set => set
                .SetProperty(r => r.State, IdempotencyState.Completed)
                .SetProperty(r => r.StatusCode, response.StatusCode)
                .SetProperty(r => r.ContentType, response.ContentType)
                .SetProperty(r => r.Location, response.Location)
                .SetProperty(r => r.ETag, response.ETag)
                .SetProperty(r => r.Body, response.Body));

    /// <summary>Only an uncommitted claim: a committed command must never run again.</summary>
    public static Task ReleaseAsync(this PlatformDbContext db, IdempotentRequest claim) =>
        db.IdempotentRequests
            .Where(r => r.Subject == claim.Subject && r.Key == claim.Key && r.State == IdempotencyState.Claimed)
            .ExecuteDeleteAsync();
}
