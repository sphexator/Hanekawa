using System;
using System.Collections.Generic;
using Hanekawa.Database.Tables.Account;
using Hanekawa.Database.Tables.Account.Profile;
using Hanekawa.Database.Tables.Account.Stores;
using Hanekawa.Database.Tables.Config.Guild;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Database
{
    public partial class DbService
    {
        public DbSet<CurrencyConfig> CurrencyConfigs { get; set; }
        // Stores
        public DbSet<ServerStore> ServerStores { get; set; }
        
        public DbSet<Background> Backgrounds { get; set; }

        private static void InventoryStoreBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CurrencyConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.Property(e => e.EmoteCurrency).HasDefaultValue(false);
                x.Property(e => e.SpecialEmoteCurrency).HasDefaultValue(false);
            });

            modelBuilder.Entity<ServerStore>(x =>
            {
                x.HasKey(e => new {e.GuildId, e.RoleId});
            });
            
            modelBuilder.Entity<Background>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                x.HasData(new List<Background>
                {
                    new ()
                    {
                        Id = Guid.NewGuid(),
                        BackgroundUrl = "https://i.imgur.com/epIb29P.png"
                    },
                    new ()
                    {
                        Id = Guid.NewGuid(),
                        BackgroundUrl = "https://i.imgur.com/04PbzvT.png"
                    },
                    new ()
                    {
                        Id = Guid.NewGuid(),
                        BackgroundUrl = "https://i.imgur.com/5ojmh76.png"
                    },
                    new ()
                    {
                        Id = Guid.NewGuid(),
                        BackgroundUrl = "https://i.imgur.com/OAMpNDh.png"
                    },
                    new ()
                    {
                        Id = Guid.NewGuid(),
                        BackgroundUrl = "https://i.imgur.com/KXO5bx5.png"
                    },
                    new ()
                    {
                        Id = Guid.NewGuid(),
                        BackgroundUrl = "https://i.imgur.com/5h5zZ7C.png"
                    }
                });
            });
        }
    }
}