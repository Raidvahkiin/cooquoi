using Cooquoi.Platform.Idempotency;
using Cooquoi.Platform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Platform.Tests.Idempotency;

public sealed class IdempotencyCleanupTests(ApiFixture api)
{
    [Fact]
    public async Task Keys_older_than_seven_days_are_deleted_and_newer_ones_kept()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var now = DateTimeOffset.UtcNow;
        var subject = $"cleanup-{Guid.NewGuid():N}";
        await using var db = api.CreateDbContext();
        db.IdempotentRequests.AddRange(Key(subject, now.AddDays(-8)), Key(subject, now.AddDays(-6)));
        await db.SaveChangesAsync(cancellationToken);

        await IdempotencyCleanup.DeleteExpiredAsync(db, now, cancellationToken);

        var left = await db.IdempotentRequests.AsNoTracking().Where(r => r.Subject == subject).ToListAsync(cancellationToken);
        left.ShouldHaveSingleItem().CreatedAt.ShouldBe(now.AddDays(-6), TimeSpan.FromMilliseconds(1));
    }

    private static IdempotentRequest Key(string subject, DateTimeOffset createdAt) => new()
    {
        Subject = subject,
        Key = Guid.NewGuid(),
        RequestHash = "hash",
        State = IdempotencyState.Completed,
        CreatedAt = createdAt,
        ClaimedAt = createdAt,
    };
}
