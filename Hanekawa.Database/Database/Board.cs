using Hanekawa.Database.Tables.BoardConfig;
using Hanekawa.Database.Tables.Config.Guild;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Database
{
    public partial class DbService
    {
        public DbSet<BoardConfig> BoardConfigs { get; set; }
        public DbSet<Board> Boards { get; set; }

        private static void BoardBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BoardConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
            modelBuilder.Entity<Board>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.MessageId});
            });
        }
    }
}