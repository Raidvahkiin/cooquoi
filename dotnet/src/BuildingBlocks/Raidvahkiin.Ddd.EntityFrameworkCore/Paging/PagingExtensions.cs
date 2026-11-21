using Microsoft.EntityFrameworkCore;

namespace Raidvahkiin.Ddd.EntityFrameworkCore.Paging;

public static class PagingExtensions
{
    /// <summary>Count query plus <c>Skip</c>/<c>Take</c> on an already sorted query.</summary>
    public static async Task<Paged<T>> ToPageAsync<T>(this IQueryable<T> query, PageRequest request, CancellationToken cancellationToken)
    {
        var totalCount = await query.LongCountAsync(cancellationToken);
        var items = await query.Skip(request.Skip).Take(request.PageSize).ToListAsync(cancellationToken);
        return new Paged<T>(items, request.Page, request.PageSize, totalCount);
    }
}
