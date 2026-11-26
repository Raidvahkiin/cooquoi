namespace Cooquoi.Context.Foodventory.Persistence;

/// <summary><c>unaccent</c> is only STABLE, so an IMMUTABLE wrapper (created by the migration) backs the FV1 generated column and its index.</summary>
public static class PantryNameKey
{
    public const string UnaccentFunction = "immutable_unaccent";

    public const string CreateUnaccentFunctionSql = $"""
        CREATE OR REPLACE FUNCTION {FoodventoryDbContext.Schema}.{UnaccentFunction}(text) RETURNS text
        LANGUAGE sql IMMUTABLE PARALLEL SAFE STRICT
        AS $$ SELECT public.unaccent('public.unaccent'::regdictionary, $1) $$;
        """;
}
