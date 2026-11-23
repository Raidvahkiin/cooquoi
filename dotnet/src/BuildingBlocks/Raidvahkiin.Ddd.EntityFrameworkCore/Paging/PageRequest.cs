namespace Raidvahkiin.Ddd.EntityFrameworkCore.Paging;

/// <summary>Offset paging, 1-based. The page size is capped by the caller's contract.</summary>
public sealed record PageRequest(int Page, int PageSize)
{
    /// <summary>In <c>long</c> then capped: a huge page number would overflow <c>int</c> into a negative offset.</summary>
    public int Skip => (int)Math.Min((long)(Page - 1) * PageSize, int.MaxValue);
}
