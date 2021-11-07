using Hanekawa.Entities.Advertise;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Infrastructure
{
    public partial class DbService
    {
        private static void AdvertisementBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TopGG>(x =>
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