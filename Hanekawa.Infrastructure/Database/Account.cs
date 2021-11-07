using Hanekawa.Entities.Account;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Infrastructure
{
    public partial class DbService
    {
        private static void AccountBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.UserId});
                x.Property(e => e.Decay).HasDefaultValue(0);
            });
            modelBuilder.Entity<AccountGlobal>(x => { x.HasKey(e => e.UserId); });
        }
    }
}