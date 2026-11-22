using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Cooquoi.AppHost.Resources;

internal static class Cache
{
    public const string WebappUser = "webapp";

    private const string PlainEndpoint = "secondary";
    private const string AclTemplatePath = "/etc/redis/users.acl.template";
    private const string AclPath = "/tmp/users.acl";

    /// <summary>
    /// Redis for webapp sessions (ADR 0019): memory-capped, no persistence, default user off,
    /// one ACL user limited to <c>session:*</c>.
    /// </summary>
    /// <remarks>
    /// The ACL file holds the password as a SHA-256 hash and Aspire secrets are only known at
    /// startup, so the mounted file is a template: the AppHost hashes the password and the entrypoint
    /// writes the hash (<c>REDIS_WEBAPP_PASSWORD_SHA256</c>) into a writable copy before starting
    /// redis-server. The ACL file cannot hold comments: <c>+hello</c> and <c>+auth</c> (beyond get/set/expire/del)
    /// are the client handshake, <c>+ping</c> the health check. The clear password never reaches the container's env or args.
    /// </remarks>
    public static IResourceBuilder<RedisResource> AddCooquoiCache(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<ParameterResource> webappPassword)
    {
        var aclTemplate = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", "redis", "users.acl"));

        const string script =
            $"sed \"s/__WEBAPP_PASSWORD_SHA256__/$REDIS_WEBAPP_PASSWORD_SHA256/\" {AclTemplatePath} > {AclPath} && " +
            $"exec redis-server --aclfile {AclPath} --maxmemory 64mb --maxmemory-policy volatile-ttl " +
            "--save \"\" --appendonly no";

        var cache = builder.AddRedis("cache")
            .WithImageTag("8.2")
            .WithBindMount(aclTemplate, AclTemplatePath, isReadOnly: true)
            .WithEnvironment(async context =>
            {
                var password = await webappPassword.Resource.GetValueAsync(context.CancellationToken);
                var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(password!)));
                context.EnvironmentVariables["REDIS_WEBAPP_PASSWORD_SHA256"] = hash;
            })
            .WithEntrypoint("/bin/sh")
            .WithArgs(context =>
            {
                // Safe to clear: Aspire's own args are only the TLS flags, and with a custom entrypoint
                // it appends them to this script (verified: the "tcp" endpoint serves rediss://).
                context.Args.Clear();
                context.Args.Add(script);
            });

        // The default user is off, so the built-in health check (default user) would never pass:
        // probe as the webapp user, over the plain endpoint (the "tcp" one serves TLS on the dev certificate).
        foreach (var check in cache.Resource.Annotations.OfType<HealthCheckAnnotation>().ToList())
        {
            cache.Resource.Annotations.Remove(check);
        }

        const string checkName = "cache_webapp_check";
        builder.Services.AddHealthChecks().AddAsyncCheck(checkName, async cancellationToken =>
        {
            var endpoint = cache.GetEndpoint(PlainEndpoint);
            var password = await webappPassword.Resource.GetValueAsync(cancellationToken);
            // A raw RESP probe: client libraries also send ECHO and others, which the ACL denies.
            // AUTH goes in cleartext to the plain endpoint, which is fine for local dev.
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(3));
            using var client = new TcpClient();
            await client.ConnectAsync(endpoint.Host, endpoint.Port, timeout.Token);
            await using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII);
            await stream.WriteAsync(Encoding.ASCII.GetBytes($"AUTH {WebappUser} {password}\r\nPING\r\n"), timeout.Token);
            var auth = await reader.ReadLineAsync(timeout.Token);
            var ping = auth == "+OK" ? await reader.ReadLineAsync(timeout.Token) : null;
            if (ping != "+PONG")
            {
                return HealthCheckResult.Unhealthy($"Redis answered AUTH with '{auth}' and PING with '{ping}' as the webapp user.");
            }

            return HealthCheckResult.Healthy();
        });
        return cache.WithHealthCheck(checkName);
    }

    /// <summary>Connection URL for the webapp's ACL user: <c>rediss://webapp:password@host:port</c> (TLS on the dev certificate).</summary>
    /// <remarks>The password is not URL-encoded: it relies on the secret having <c>Special = false</c> (see <c>Secrets</c>).</remarks>
    public static ReferenceExpression WebappUrl(
        IResourceBuilder<RedisResource> cache,
        IResourceBuilder<ParameterResource> webappPassword)
    {
        var endpoint = cache.GetEndpoint("tcp");
        return ReferenceExpression.Create($"rediss://{WebappUser}:{webappPassword}@{endpoint.Property(EndpointProperty.Host)}:{endpoint.Property(EndpointProperty.Port)}");
    }
}
