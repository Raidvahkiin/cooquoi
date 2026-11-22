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
        var keycloak = builder.AddKeycloak("keycloak")
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
    public static ReferenceExpression IssuerUrl(IResourceBuilder<KeycloakResource> keycloak) =>
        ReferenceExpression.Create($"{keycloak.GetEndpoint("http")}/realms/{Realm}");

    private static string KeycloakFile(IDistributedApplicationBuilder builder, string relativePath) =>
        Path.Combine(builder.AppHostDirectory, "..", "keycloak", relativePath);
}
