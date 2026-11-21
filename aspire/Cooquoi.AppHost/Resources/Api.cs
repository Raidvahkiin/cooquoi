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
            .WithEnvironment("Authentication__Authority", Identity.IssuerUrl(keycloak));

    private static string ApiProjectPath(IDistributedApplicationBuilder builder) =>
        Path.Combine(builder.AppHostDirectory, "..", "..", "dotnet", "src", "Cooquoi.Api", "Cooquoi.Api.csproj");
}
