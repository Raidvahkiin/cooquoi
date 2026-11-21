namespace Cooquoi.AppHost.Resources;

internal static class Database
{
    public const string Name = "cooquoi";

    /// <summary>One Postgres database for every context, one schema per context (ADR 0010).</summary>
    public static IResourceBuilder<PostgresDatabaseResource> AddCooquoiDatabase(
        this IDistributedApplicationBuilder builder) =>
        builder.AddPostgres("postgres")
            .WithDataVolume()
            .AddDatabase(Name);
}
