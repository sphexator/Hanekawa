using System.Linq;
using Disqord;
using Hanekawa.Entities.Giveaway;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Infrastructure
{
    public partial class DbService
    {
        private static void GiveawayBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Giveaway>(x =>
            {
                x.HasKey(e => new { e.Id });
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                x.HasMany(e => e.Participants)
                    .WithOne(e => e.Giveaway)
                    .HasForeignKey(e => e.GiveawayId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<GiveawayParticipant>(x =>
            {
                x.HasKey(e => new { e.Id });
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<GiveawayHistory>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
            });
        }
    }
}