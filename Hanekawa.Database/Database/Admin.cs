using Hanekawa.Database.Tables.Administration;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Database.Tables.Moderation;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Database
{
    public partial class DbService
    {
        public DbSet<AdminConfig> AdminConfigs { get; set; }
        public DbSet<Blacklist> Blacklists { get; set; }

        public DbSet<ModLog> ModLogs { get; set; }
        public DbSet<MuteTimer> MuteTimers { get; set; }
        public DbSet<Warn> Warns { get; set; }
        
        private static void AdministrationBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdminConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
            modelBuilder.Entity<ModLog>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
            });
            modelBuilder.Entity<MuteTimer>(x =>
            {
                x.HasKey(e => new {e.UserId, e.GuildId});
            });
            modelBuilder.Entity<Warn>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
            });
        }
    }
}