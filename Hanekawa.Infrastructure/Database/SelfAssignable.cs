using Hanekawa.Entities.Config;
using Hanekawa.Entities.Config.SelfAssign;
using Hanekawa.Entities.SelfAssign;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Infrastructure
{
    public partial class DbService
    {
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

            modelBuilder.Entity<SelfAssignGroup>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                x.Property(e => e.Name).IsRequired();
                x.HasMany(e => e.Roles)
                    .WithOne(e => e.Group)
                    .HasForeignKey(e => e.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<SelfAssignItem>(x =>
            {
                x.HasKey(e => e.RoleId);
            });
        }
    }
}