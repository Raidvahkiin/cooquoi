using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Context.Foodpedia.Persistence.Projections;

/// <summary>
/// Postgres search functions usable in LINQ (trigram ones come with Npgsql: <c>EF.Functions.Trigrams…</c>). <c>unaccent</c> is only STABLE, so an IMMUTABLE wrapper
/// (created by the migration) backs the generated column and its index.
/// </summary>
public static class SearchFunctions
{
    public const string UnaccentFunction = "immutable_unaccent";

    public const string CreateUnaccentFunctionSql = $"""
        CREATE OR REPLACE FUNCTION {FoodpediaDbContext.Schema}.{UnaccentFunction}(text) RETURNS text
        LANGUAGE sql IMMUTABLE PARALLEL SAFE STRICT
        AS $$ SELECT public.unaccent('public.unaccent'::regdictionary, $1) $$;
        """;

    public static string Unaccent(string text) => throw new InvalidOperationException($"{nameof(Unaccent)} runs only in SQL ({text})");

    internal static void Register(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDbFunction(typeof(SearchFunctions).GetMethod(nameof(Unaccent))!)
            .HasName(UnaccentFunction).HasSchema(FoodpediaDbContext.Schema);
    }
}
