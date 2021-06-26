using System.Linq;
using Disqord;
using Hanekawa.Database.Tables.Giveaway;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Database
{
    public partial class DbService
    {
        public DbSet<Giveaway> Giveaways { get; set; }
        public DbSet<GiveawayParticipant> GiveawayParticipants { get; set; }
        public DbSet<GiveawayHistory> GiveawayHistories { get; set; }
        
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
                x.Property(e => e.Winner).HasConversion(c => c.Select(item => item.RawValue).ToArray(),
                    wops => wops.Select(item => new Snowflake(item)).ToArray());
            });
        }
    }
}