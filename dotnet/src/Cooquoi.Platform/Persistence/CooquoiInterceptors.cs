using Cooquoi.Platform.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Raidvahkiin.Ddd;
using Raidvahkiin.Ddd.EntityFrameworkCore.Interceptors;

namespace Cooquoi.Platform.Persistence;

public static class CooquoiInterceptors
{
    /// <summary>
    /// Every context's <c>DbContext</c>: aggregate version and audit, idempotency commit, and failed
    /// saves as domain errors (concurrency conflicts, plus the context's own unique constraints).
    /// </summary>
    public static DbContextOptionsBuilder AddCooquoiInterceptors(
        this DbContextOptionsBuilder options, IServiceProvider services, IReadOnlyDictionary<string, DomainError>? uniqueConstraints = null) =>
        options.AddInterceptors(
            services.GetRequiredService<AggregateSaveInterceptor>(),
            services.GetRequiredService<IdempotencyCommitInterceptor>(),
            new ConstraintViolationInterceptor(uniqueConstraints ?? new Dictionary<string, DomainError>()));
}
