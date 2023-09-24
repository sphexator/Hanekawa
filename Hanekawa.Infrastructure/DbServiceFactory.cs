using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hanekawa.Infrastructure
{
    internal class DbServiceFactory : IDesignTimeDbContextFactory<DbService>
    {
        public DbService CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<DbService>();
            builder.UseNpgsql(
                "Server=localhost; Port=5432; Database=hanekawa-development; Userid=postgres;Password=1023;");
            return new(builder.Options);
        }
    }
}