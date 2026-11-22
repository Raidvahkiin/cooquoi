using System.Net.Sockets;
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
    /// startup, so the mounted file is a template: the entrypoint hashes the password
    /// (<c>REDIS_WEBAPP_PASSWORD</c>) into a writable copy before starting redis-server.
    /// </remarks>
    public static IResourceBuilder<RedisResource> AddCooquoiCache(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<ParameterResource> webappPassword)
    {
        var aclTemplate = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", "redis", "users.acl"));

        const string script =
            "hash=$(printf %s \"$REDIS_WEBAPP_PASSWORD\" | sha256sum | cut -d' ' -f1) && " +
            $"sed \"s/__WEBAPP_PASSWORD_SHA256__/$hash/\" {AclTemplatePath} > {AclPath} && " +
            $"exec redis-server --aclfile {AclPath} --maxmemory 64mb --maxmemory-policy volatile-ttl " +
            "--save \"\" --appendonly no";

        var cache = builder.AddRedis("cache")
            .WithImageTag("8.2")
            .WithBindMount(aclTemplate, AclTemplatePath, isReadOnly: true)
            .WithEnvironment("REDIS_WEBAPP_PASSWORD", webappPassword)
            .WithEntrypoint("/bin/sh")
            .WithArgs(context =>
            {
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
            using var client = new TcpClient();
            await client.ConnectAsync(endpoint.Host, endpoint.Port, cancellationToken);
            var stream = client.GetStream();
            var command = $"AUTH {WebappUser} {password}\r\nPING\r\n";
            await stream.WriteAsync(Encoding.ASCII.GetBytes(command), cancellationToken);
            var buffer = new byte[64];
            var read = 0;
            while (read < buffer.Length && !Encoding.ASCII.GetString(buffer, 0, read).EndsWith("+PONG\r\n"))
            {
                var count = await stream.ReadAsync(buffer.AsMemory(read), cancellationToken);
                if (count == 0) break;
                read += count;
            }
            if (!Encoding.ASCII.GetString(buffer, 0, read).Equals("+OK\r\n+PONG\r\n"))
            {
                return HealthCheckResult.Unhealthy("Redis did not answer AUTH and PING as the webapp user.");
            }

            return HealthCheckResult.Healthy();
        });
        return cache.WithHealthCheck(checkName);
    }

    /// <summary>Connection URL for the webapp's ACL user: <c>rediss://webapp:password@host:port</c> (TLS on the dev certificate).</summary>
    public static ReferenceExpression WebappUrl(
        IResourceBuilder<RedisResource> cache,
        IResourceBuilder<ParameterResource> webappPassword)
    {
        var endpoint = cache.GetEndpoint("tcp");
        return ReferenceExpression.Create($"rediss://{WebappUser}:{webappPassword}@{endpoint.Property(EndpointProperty.Host)}:{endpoint.Property(EndpointProperty.Port)}");
    }
}
