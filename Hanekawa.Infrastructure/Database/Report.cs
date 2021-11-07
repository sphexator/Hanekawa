using Hanekawa.Entities.Moderation;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Infrastructure
{
    public partial class DbService
    {
        private static void ReportBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Report>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
            });
        }
    }
}