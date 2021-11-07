using System;
using System.Collections.Generic;
using Hanekawa.Entities.Account.Profile;
using Hanekawa.Entities.Account.Stores;
using Hanekawa.Entities.Config.Guild;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Infrastructure
{
    public partial class DbService
    {
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