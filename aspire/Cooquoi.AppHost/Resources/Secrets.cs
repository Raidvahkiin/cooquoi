namespace Cooquoi.AppHost.Resources;

internal static class Secrets
{
    /// <summary>A random secret generated on first run and kept in the AppHost's user secrets.</summary>
    public static IResourceBuilder<ParameterResource> AddGeneratedSecret(
        this IDistributedApplicationBuilder builder,
        string name) =>
        builder.AddParameter(
            name,
            new GenerateParameterDefault { MinLength = 48, Special = false },
            secret: true,
            persist: true);
}
