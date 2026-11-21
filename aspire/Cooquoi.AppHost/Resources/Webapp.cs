namespace Cooquoi.AppHost.Resources;

internal static class Webapp
{
    /// <summary>Fixed port: it is the OIDC redirect target registered in the Keycloak realm.</summary>
    public const int Port = 3000;
    public static readonly string Url = $"http://localhost:{Port}";

    public static IResourceBuilder<NextJsAppResource> AddCooquoiWebapp(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<DotnetProjectResource> api,
        IResourceBuilder<KeycloakResource> keycloak,
        IResourceBuilder<ParameterResource> webappClientSecret)
    {
        var sessionPassword = builder.AddGeneratedSecret("webapp-session-password");
        var webappDirectory = Path.Combine(builder.AppHostDirectory, "..", "..", "ts", "apps", "cooquoi-webapp");

        return builder.AddNextJsApp("webapp", webappDirectory)
            .WithPnpm(install: false)
            .WithEndpoint("http", endpoint => endpoint.Port = Port)
            .WaitFor(api)
            .WaitFor(keycloak)
            .WithEnvironment("APP_URL", Url)
            .WithEnvironment("API_BASE_URL", api.GetEndpoint("http"))
            .WithEnvironment("OIDC_ISSUER", Identity.IssuerUrl(keycloak))
            .WithEnvironment("OIDC_CLIENT_ID", Identity.WebappClientId)
            .WithEnvironment("OIDC_CLIENT_SECRET", webappClientSecret)
            .WithEnvironment("SESSION_PASSWORD", sessionPassword);
    }
}
