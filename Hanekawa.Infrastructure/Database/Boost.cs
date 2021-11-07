using Hanekawa.Entities.Config.Guild;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Infrastructure
{
    public partial class DbService
    {
        private static void BoostBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BoostConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
        }
    }
}