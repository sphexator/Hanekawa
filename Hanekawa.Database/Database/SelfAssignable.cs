using Hanekawa.Database.Tables.Config;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Database
{
    public partial class DbService
    {
        public DbSet<SelfAssignAbleRole> SelfAssignAbleRoles { get; set; }
        public DbSet<SelfAssignReactionRole> SelfAssignReactionRoles { get; set; }

        private static void SelfAssignableBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SelfAssignAbleRole>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.RoleId});
            });
            modelBuilder.Entity<SelfAssignReactionRole>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.ChannelId, e.MessageId});
            });
        }
    }
}