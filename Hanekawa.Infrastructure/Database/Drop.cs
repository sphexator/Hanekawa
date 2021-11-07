using Hanekawa.Entities.Config;
using Hanekawa.Entities.Config.Guild;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Infrastructure
{
    public partial class DbService
    {
        private static void DropBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DropConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
        }
    }
}