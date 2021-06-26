using Hanekawa.Database.Tables.Config;
using Hanekawa.Database.Tables.Config.Guild;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Database
{
    public partial class DbService
    {
        public DbSet<WelcomeBanner> WelcomeBanners { get; set; }
        public DbSet<WelcomeConfig> WelcomeConfigs { get; set; }

        private static void WelcomeBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WelcomeConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
            modelBuilder.Entity<WelcomeBanner>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.Id});
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                x.Property(e => e.AvatarSize).HasDefaultValue(60);
                x.Property(e => e.AviPlaceX).HasDefaultValue(10);
                x.Property(e => e.AviPlaceY).HasDefaultValue(10);
                x.Property(e => e.TextSize).HasDefaultValue(33);
                x.Property(e => e.TextPlaceX).HasDefaultValue(245);
                x.Property(e => e.TextPlaceY).HasDefaultValue(40);
            });
        }
    }
}