using Cooquoi.Platform.Auditing;
using Cooquoi.Platform.Concurrency;
using Cooquoi.Platform.Idempotency;
using Cooquoi.Platform.Persistence;
using Cooquoi.Platform.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raidvahkiin.Ddd.EntityFrameworkCore;
using Raidvahkiin.Ddd.EntityFrameworkCore.Interceptors;
using Wolverine.Http.FluentValidation;

namespace Cooquoi.Platform;

/// <summary>Cross-cutting infrastructure shared by every context.</summary>
public static class PlatformModule
{
    public static IHostApplicationBuilder AddCooquoiPlatform(this IHostApplicationBuilder builder, string connectionName)
    {
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<IAuditActor, CurrentUserAuditActor>();
        builder.Services.AddSingleton<AggregateSaveInterceptor>();
        builder.Services.AddSingleton<IdempotencyCommitInterceptor>();
        builder.Services.AddHostedService<IdempotencyCleanup>();
        builder.Services.AddSingleton(typeof(IProblemDetailSource<>), typeof(ContractProblemDetailSource<>));
        builder.Services.AddDbContext<PlatformDbContext>(options =>
            options.UseSchemaNpgsql(builder.Configuration.RequiredConnectionString(connectionName), PlatformDbContext.Schema));
        return builder;
    }

    /// <summary>
    /// After authorization, so only authorized requests claim an idempotency key. The concurrency
    /// retry runs inside it: the key records the final outcome, not a lost attempt.
    /// </summary>
    public static IApplicationBuilder UseCooquoiCommandPolicies(this IApplicationBuilder app) =>
        app.UseMiddleware<IdempotencyMiddleware>().UseMiddleware<ConcurrencyRetryMiddleware>();

    /// <summary>Development only: production applies a migration bundle before start (ADR 0014).</summary>
    public static async Task MigratePlatformDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<PlatformDbContext>().Database.MigrateAsync(cancellationToken);
    }

    public static string RequiredConnectionString(this IConfiguration configuration, string connectionName) =>
        configuration.GetConnectionString(connectionName)
            ?? throw new InvalidOperationException($"Missing connection string '{connectionName}'");
}
