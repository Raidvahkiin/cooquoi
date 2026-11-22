namespace Cooquoi.AppHost.Resources;

internal static class Webapp
{
    /// <summary>Fixed port: it is the OIDC redirect target registered in the Keycloak realm.</summary>
    public const int Port = 3000;
    public static readonly string Url = $"https://localhost:{Port}";

    public static IResourceBuilder<NextJsAppResource> AddCooquoiWebapp(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<DotnetProjectResource> api,
        IResourceBuilder<KeycloakResource> keycloak,
        IResourceBuilder<RedisResource> cache,
        IResourceBuilder<ParameterResource> redisWebappPassword,
        IResourceBuilder<ParameterResource> webappClientSecret)
    {
        var sessionPassword = builder.AddGeneratedSecret("webapp-session-password");
        var webappDirectory = Path.Combine(builder.AppHostDirectory, "..", "..", "ts", "apps", "cooquoi-webapp");

        return builder.AddNextJsApp("webapp", webappDirectory)
            .WithPnpm(install: false)
            // The dev script serves https on the dev certificate and passes it to the server as
            // NODE_EXTRA_CA_CERTS (--experimental-https-ca); AddNextJsApp names its endpoint "http".
            .WithEndpoint("http", endpoint =>
            {
                endpoint.Port = Port;
                endpoint.UriScheme = "https";
            })
            .WaitFor(api)
            .WaitFor(keycloak)
            .WaitFor(cache)
            .WithEnvironment("APP_URL", Url)
            .WithEnvironment("API_BASE_URL", api.GetEndpoint("https"))
            .WithEnvironment("OIDC_ISSUER", Identity.IssuerUrl(keycloak))
            .WithEnvironment("OIDC_CLIENT_ID", Identity.WebappClientId)
            .WithEnvironment("OIDC_CLIENT_SECRET", webappClientSecret)
            .WithEnvironment("REDIS_URL", Cache.WebappUrl(cache, redisWebappPassword))
            .WithEnvironment("SESSION_PASSWORD", sessionPassword);
    }
}
