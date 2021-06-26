using Hanekawa.Database.Tables.Advertise;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Database
{
    public partial class DbService
    {
        public DbSet<DblAuth> DblAuths { get; set; }
        public DbSet<VoteLog> VoteLogs { get; set; }
        
        private static void AdvertisementBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DblAuth>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.AuthKey).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<VoteLog>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
        }
    }
}