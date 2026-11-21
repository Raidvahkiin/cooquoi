using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Raidvahkiin.Ddd.EntityFrameworkCore;

namespace Cooquoi.Platform.Persistence;

/// <summary>Used only by <c>dotnet ef</c> to generate migrations (no database connection needed).</summary>
internal sealed class PlatformDbContextFactory : IDesignTimeDbContextFactory<PlatformDbContext>
{
    public PlatformDbContext CreateDbContext(string[] args) =>
        new((DbContextOptions<PlatformDbContext>)new DbContextOptionsBuilder<PlatformDbContext>()
            .UseSchemaNpgsql("Host=localhost;Database=cooquoi", PlatformDbContext.Schema)
            .Options);
}
