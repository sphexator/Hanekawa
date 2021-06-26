using Hanekawa.Database.Tables.Moderation;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Database
{
    public partial class DbService
    {
        public DbSet<Report> Reports { get; set; }
        
        private static void ReportBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Report>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
            });
        }
    }
}