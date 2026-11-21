namespace Raidvahkiin.Ddd.EntityFrameworkCore.Paging;

/// <summary>Offset paging, 1-based. The page size is capped by the caller's contract.</summary>
public sealed record PageRequest(int Page, int PageSize)
{
    public int Skip => (Page - 1) * PageSize;
}
