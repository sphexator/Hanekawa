using Hanekawa.Database.Tables.Account;
using Hanekawa.Database.Tables.Config;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Database
{
    public partial class DbService
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountGlobal> AccountGlobals { get; set; }

        private static void AccountBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.UserId});
                x.Property(e => e.Decay).HasDefaultValue(0);
            });
            modelBuilder.Entity<AccountGlobal>(x =>
            {
                x.HasKey(e => e.UserId);
            });
        }
    }
}