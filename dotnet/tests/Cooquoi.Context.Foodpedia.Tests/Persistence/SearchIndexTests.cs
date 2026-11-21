using Cooquoi.Context.Foodpedia.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Cooquoi.Context.Foodpedia.Tests.Persistence;

/// <summary>The search predicates (<c>LIKE '%…%'</c> and <c>&lt;%</c>) can use the trigram index, not only a scan.</summary>
public sealed class SearchIndexTests(ApiFixture api)
{
    [Theory]
    [InlineData("search_text LIKE '%' || foodpedia.immutable_unaccent('cá hồi') || '%'")]
    [InlineData("foodpedia.immutable_unaccent('corriander') <% search_text")]
    public async Task The_trigram_index_serves_the_search_predicate(string predicate)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = api.CreateDbContext();
        var connection = (NpgsqlConnection)db.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        // Tiny tables are always scanned: forbid scans to see whether an index can answer at all.
        await using (var disable = new NpgsqlCommand("SET LOCAL enable_seqscan = off", connection, transaction))
        {
            await disable.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var explain = new NpgsqlCommand(
            $"EXPLAIN SELECT ingredient_id FROM foodpedia.ingredient_search_terms WHERE {predicate}", connection, transaction);
        var plan = new List<string>();
        await using (var reader = await explain.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                plan.Add(reader.GetString(0));
            }
        }

        string.Join('\n', plan).ShouldContain("ix_ingredient_search_terms_search_text");
    }
}
