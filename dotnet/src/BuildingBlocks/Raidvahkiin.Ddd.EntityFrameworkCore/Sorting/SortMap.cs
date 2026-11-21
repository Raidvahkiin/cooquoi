using System.Linq.Expressions;

namespace Raidvahkiin.Ddd.EntityFrameworkCore.Sorting;

/// <summary>
/// Whitelist of sortable fields of one read model (ADR 0010). Sort strings are looked up,
/// never interpreted: <c>"name"</c> ascending, <c>"-name"</c> descending.
/// </summary>
public sealed class SortMap<T>
{
    private readonly Dictionary<string, LambdaExpression> _fields = new(StringComparer.Ordinal);

    public SortMap<T> Add<TKey>(string field, Expression<Func<T, TKey>> key)
    {
        _fields.Add(field, key);
        return this;
    }

    public IEnumerable<string> Fields => _fields.Keys;

    /// <summary>Applies the requested sorts in order, then <paramref name="tieBreaker"/> for a stable order.</summary>
    public IQueryable<T> Apply<TKey>(IQueryable<T> query, IReadOnlyList<string> sorts, Expression<Func<T, TKey>> tieBreaker)
    {
        IOrderedQueryable<T>? ordered = null;
        foreach (var sort in sorts)
        {
            var descending = sort.StartsWith('-');
            var field = descending ? sort[1..] : sort;
            if (!_fields.TryGetValue(field, out var key))
            {
                throw CommonError.InvalidSort.ToException(("sort", sort), ("allowed", string.Join(", ", Fields)));
            }

            ordered = OrderBy(ordered ?? query, key, new SortStep(ordered is not null, descending));
        }

        return ordered is null ? query.OrderBy(tieBreaker) : ordered.ThenBy(tieBreaker);
    }

    private static IOrderedQueryable<T> OrderBy(IQueryable<T> query, LambdaExpression key, SortStep step)
    {
        var method = (step.Then, step.Descending) switch
        {
            (false, false) => nameof(Queryable.OrderBy),
            (false, true) => nameof(Queryable.OrderByDescending),
            (true, false) => nameof(Queryable.ThenBy),
            (true, true) => nameof(Queryable.ThenByDescending),
        };
        var call = Expression.Call(
            typeof(Queryable), method, [typeof(T), key.ReturnType], query.Expression, Expression.Quote(key));
        return (IOrderedQueryable<T>)query.Provider.CreateQuery<T>(call);
    }

    private readonly record struct SortStep(bool Then, bool Descending);
}
