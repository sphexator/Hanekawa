using System;
using Hanekawa.Entities.Premium;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Infrastructure
{
    public partial class DbService
    {
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