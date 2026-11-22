namespace Cooquoi.AppHost.Resources;

internal static class Identity
{
    public const string Realm = "cooquoi";
    public const string WebappClientId = "cooquoi-webapp";

    /// <summary>Keycloak with the versioned realm (ADR 0004); dev users only in development.</summary>
    public static IResourceBuilder<KeycloakResource> AddCooquoiKeycloak(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<ParameterResource> webappClientSecret)
    {
        // Keycloak on the .NET dev certificate, so Node and .NET trust it (ADR 0018); the API is still experimental.
#pragma warning disable ASPIRECERTIFICATES001
        var keycloak = builder.AddKeycloak("keycloak")
            .WithHttpsDeveloperCertificate()
#pragma warning restore ASPIRECERTIFICATES001
            .WithRealmImport(KeycloakFile(builder, "realms/cooquoi-realm.json"))
            .WithEnvironment("COOQUOI_WEBAPP_CLIENT_SECRET", webappClientSecret)
            .WithEnvironment("COOQUOI_WEBAPP_URL", Webapp.Url);

        if (builder.Environment.IsDevelopment())
        {
            var devUserPassword = builder.AddParameter("dev-user-password", "cooquoi-dev", secret: true);
            keycloak
                .WithRealmImport(KeycloakFile(builder, "dev-users/cooquoi-users-0.json"))
                .WithEnvironment("COOQUOI_DEV_USER_PASSWORD", devUserPassword);
        }

        return keycloak;
    }

    /// <summary>Issuer URL of the Cooquoi realm, as seen by browsers and services.</summary>
    /// <remarks>On the dev certificate Keycloak keeps its endpoint named "http" but serves https.</remarks>
    public static ReferenceExpression IssuerUrl(IResourceBuilder<KeycloakResource> keycloak) =>
        ReferenceExpression.Create($"{keycloak.GetEndpoint("http")}/realms/{Realm}");

    private static string KeycloakFile(IDistributedApplicationBuilder builder, string relativePath) =>
        Path.Combine(builder.AppHostDirectory, "..", "keycloak", relativePath);
}
