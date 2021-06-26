using Hanekawa.Database.Tables.Config.Guild;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Database
{
    public partial class DbService
    {
        public DbSet<BoostConfig> BoostConfigs { get; set; }
        private static void BoostBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BoostConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
        }
    }
}