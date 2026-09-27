using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MedMatch.Infrastructure.Persistence;

/// <summary>EF CLI'nin API'yi ayağa kaldırmadan migration üretebilmesi için.</summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var conn = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5433;Database=medmatch;Username=medmatch;Password=medmatch";
        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(conn).Options;
        return new AppDbContext(options);
    }
}
