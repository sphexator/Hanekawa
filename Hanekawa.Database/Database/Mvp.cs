using System;
using Hanekawa.Database.Tables.Premium;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Database
{
    public partial class DbService
    {
        public DbSet<MvpConfig> MvpConfigs { get; set; }
        private static void MvpBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MvpConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.Day).HasConversion(
                    v => v.ToString(),
                    v => (DayOfWeek) Enum.Parse(typeof(DayOfWeek), v));
                x.Property(e => e.Disabled).HasDefaultValue(true);
            });
        }
    }
}