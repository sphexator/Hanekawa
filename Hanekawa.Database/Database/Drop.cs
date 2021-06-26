using Hanekawa.Database.Tables.Config;
using Hanekawa.Database.Tables.Config.Guild;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Database
{
    public partial class DbService
    {
        public DbSet<DropConfig> DropConfigs { get; set; }
        public DbSet<DropChannel> DropChannels { get; set; }

        private static void DropBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DropConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
            modelBuilder.Entity<DropChannel>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.ChannelId});
            });
        }
    }
}