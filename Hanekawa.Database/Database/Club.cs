using Hanekawa.Database.Tables.Club;
using Hanekawa.Database.Tables.Config.Guild;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Database
{
    public partial class DbService
    {
        public DbSet<ClubConfig> ClubConfigs { get; set; }
        public DbSet<Club> ClubInfos { get; set; }
        public DbSet<ClubUser> ClubPlayers { get; set; }
        public DbSet<ClubBlacklist> ClubBlacklists { get; set; }
        
        private static void ClubBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClubConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
            modelBuilder.Entity<Club>(x =>
            {
                x.HasKey(e => new {e.Id});
                x.Property(e => e.Id).ValueGeneratedOnAdd();

                x.HasMany(e => e.Users)
                    .WithOne(e => e.Club)
                    .HasForeignKey(e => e.ClubId)
                    .OnDelete(DeleteBehavior.Cascade);
                x.HasMany(e => e.Blacklist)
                    .WithOne(e => e.Club)
                    .HasForeignKey(e => e.ClubId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            modelBuilder.Entity<ClubUser>(x =>
            {
                x.HasKey(e => new {e.Id});
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<ClubBlacklist>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.User});
            });
        }
    }
}