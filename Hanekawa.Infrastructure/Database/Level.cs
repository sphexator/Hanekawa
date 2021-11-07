using Hanekawa.Entities.Config.Guild;
using Hanekawa.Entities.Config.Level;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Infrastructure
{
    public partial class DbService
    {
        private static void LevelBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LevelConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.BoostExpMultiplier).HasDefaultValue(1);
                x.Property(e => e.Decay).HasDefaultValue(false);
            });
            modelBuilder.Entity<LevelReward>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.Level});
                x.Property(e => e.NoDecay).HasDefaultValue(false);
            });
            modelBuilder.Entity<LevelExpEvent>(x => { x.HasKey(e => e.GuildId); });
        }
    }
}