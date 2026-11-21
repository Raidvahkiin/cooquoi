namespace Raidvahkiin.Ddd.EntityFrameworkCore.Paging;

/// <summary>One page of a list query, with the total count of matching items (contract <c>Page/// <summary>One page of a list query, with the total count of matching items.</summary>lt;T/// <summary>One page of a list query, with the total count of matching items.</summary>gt;</c>).</summary>
public sealed record Paged<T>(IReadOnlyList<T> Items, int Page, int PageSize, long TotalCount)
{
    public Paged<TResult> Map<TResult>(Func<T, TResult> map) => new([.. Items.Select(map)], Page, PageSize, TotalCount);
}
