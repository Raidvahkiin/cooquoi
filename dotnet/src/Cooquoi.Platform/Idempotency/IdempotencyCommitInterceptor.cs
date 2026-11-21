using Cooquoi.Platform.Auditing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Cooquoi.Platform.Idempotency;

/// <summary>
/// Adds "key committed" to the first save of a request holding an idempotency key, so the key
/// and the command commit together (ADR 0014 §6d): a crash afterwards can never re-run it.
/// </summary>
internal sealed class IdempotencyCommitInterceptor(IHttpContextAccessor accessor) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        MarkCommitted(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        MarkCommitted(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void MarkCommitted(DbContext? context)
    {
        var claim = IdempotencyClaim.Of(accessor.HttpContext);
        // A handler running in the request's async flow is not the command: SystemActor tells them apart.
        if (context is null || claim is null || claim.Committed || SystemActor.Current is not null || context.Model.FindEntityType(typeof(IdempotencyCommit)) is null
            || !context.ChangeTracker.HasChanges())
        {
            return;
        }

        var commit = context.Attach(new IdempotencyCommit { Subject = claim.Subject, Key = claim.Key, State = IdempotencyState.Claimed });
        commit.Property(c => c.State).CurrentValue = IdempotencyState.Committed;
        claim.Committed = true;
    }
}
