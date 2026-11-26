using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

namespace Raidvahkiin.Ddd.EntityFrameworkCore.Interceptors;

/// <summary>
/// Turns failed saves into domain errors: an optimistic concurrency failure into
/// <c>common.concurrencyConflict</c>, a known unique constraint (safety net behind a use-case
/// pre-check) into its error. Unknown failures keep their original exception.
/// </summary>
/// <remarks>
/// EF Core does not report concurrency failures to <c>SaveChangesFailed</c>, and wraps whatever
/// <c>ThrowingConcurrencyException</c> throws in a <see cref="DbUpdateException"/>; hence both hooks.
/// </remarks>
public sealed class ConstraintViolationInterceptor(IReadOnlyDictionary<string, DomainError> uniqueConstraints)
    : SaveChangesInterceptor
{
    public override InterceptionResult ThrowingConcurrencyException(
        ConcurrencyExceptionEventData eventData, InterceptionResult result) =>
        throw CommonError.ConcurrencyConflict.ToException();

    public override ValueTask<InterceptionResult> ThrowingConcurrencyExceptionAsync(
        ConcurrencyExceptionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default) =>
        throw CommonError.ConcurrencyConflict.ToException();

    public override void SaveChangesFailed(DbContextErrorEventData eventData) => ThrowMapped(eventData.Exception);

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        ThrowMapped(eventData.Exception);
        return Task.CompletedTask;
    }

    private void ThrowMapped(Exception exception)
    {
        switch (exception)
        {
            case DbUpdateException { InnerException: DomainException domain }:
                throw domain;
            case DbUpdateException
            {
                InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: { } constraint },
            } when uniqueConstraints.TryGetValue(constraint, out var error):
                throw error.ToException();
        }
    }
}
