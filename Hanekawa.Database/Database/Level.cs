using Hanekawa.Database.Tables.Config;
using Hanekawa.Database.Tables.Config.Guild;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Database
{
    public partial class DbService
    {
        public DbSet<LevelConfig> LevelConfigs { get; set; }
        public DbSet<LevelReward> LevelRewards { get; set; }
        public DbSet<LevelExpReduction> LevelExpReductions { get; set; }
        public DbSet<LevelExpEvent> LevelExpEvents { get; set; }

        private static void LevelBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LevelConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.BoostExpMultiplier).HasDefaultValue(1);
                x.Property(e => e.Decay).HasDefaultValue(false);
            });
            modelBuilder.Entity<LevelExpReduction>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.ChannelId});
            });
            modelBuilder.Entity<LevelReward>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.Level});
                x.Property(e => e.NoDecay).HasDefaultValue(false);
            });
            modelBuilder.Entity<LevelExpEvent>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
        }
    }
}