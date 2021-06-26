using Hanekawa.Database.Tables.Config;
using Hanekawa.Database.Tables.Config.Guild;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Database
{
    public partial class DbService
    {
        public DbSet<GuildConfig> GuildConfigs { get; set; }
        public DbSet<ChannelConfig> ChannelConfigs { get; set; }
        public DbSet<LoggingConfig> LoggingConfigs { get; set; }
        public DbSet<IgnoreChannel> IgnoreChannels { get; set; }
        
        private static void ConfigBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuildConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.Premium).HasDefaultValue(null);
            });
            modelBuilder.Entity<ChannelConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.HasMany(e => e.AssignReactionRoles).WithOne(e => e.Config).OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<LoggingConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
            modelBuilder.Entity<IgnoreChannel>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.ChannelId});
            });
        }
    }
}