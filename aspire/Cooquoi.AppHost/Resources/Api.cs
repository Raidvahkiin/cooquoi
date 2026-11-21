namespace Cooquoi.AppHost.Resources;

internal static class Api
{
    public static IResourceBuilder<DotnetProjectResource> AddCooquoiApi(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<PostgresDatabaseResource> database,
        IResourceBuilder<KeycloakResource> keycloak) =>
        builder.AddDotnetProject("api", ApiProjectPath(builder))
            .WithReference(database)
            .WaitFor(database)
            .WaitFor(keycloak)
            .WithEnvironment("Authentication__Authority", Identity.IssuerUrl(keycloak))
            // Without it the dashboard showed the API healthy while it had crashed at startup.
            .WithHttpHealthCheck("/health");

    private static string ApiProjectPath(IDistributedApplicationBuilder builder) =>
        Path.Combine(builder.AppHostDirectory, "..", "..", "dotnet", "src", "Cooquoi.Api", "Cooquoi.Api.csproj");
}
