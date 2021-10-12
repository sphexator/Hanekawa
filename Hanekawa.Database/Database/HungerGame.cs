using System.Collections.Generic;
using Hanekawa.Database.Tables.Account.HungerGame;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Database
{
    public partial class DbService
    {
        public DbSet<Game> HungerGames { get; set; }
        public DbSet<HungerGameCustomCharacter> HungerGameCustomChars { get; set; }
        public DbSet<HungerGameDefault> HungerGameDefaults { get; set; }
        public DbSet<HungerGameHistory> HungerGameHistories { get; set; }
        public DbSet<HungerGameProfile> HungerGameProfiles { get; set; }
        public DbSet<HungerGameStatus> HungerGameStatus { get; set; }
        
        private static void HungerGameBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<HungerGameHistory>(x =>
            {
                x.HasKey(e => e.GameId);
            });
            modelBuilder.Entity<HungerGameProfile>(x =>
            {
                x.HasKey(e => new { e.GuildId, e.UserId });
            });
            modelBuilder.Entity<HungerGameStatus>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
            modelBuilder.Entity<HungerGameCustomCharacter>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<HungerGameDefault>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                x.HasData(new List<HungerGameDefault>
                {
                    new ()
                    {
                        Id = 1,
                        Name = "Dia",
                        Avatar = "https://i.imgur.com/XMjW8Qn.png"
                    },
                    new ()
                    {
                        Id = 2,
                        Name = "Kanan",
                        Avatar = "https://i.imgur.com/7URjbvT.png"
                    },
                    new ()
                    {
                        Id = 3,
                        Name = "Yoshiko",
                        Avatar = "https://i.imgur.com/tPDON9P.png"
                    },
                    new ()
                    {
                        Id = 4,
                        Name = "Kongou",
                        Avatar = "https://i.imgur.com/dcB1loo.png"
                    },
                    new ()
                    {
                        Id = 5,
                        Name = "Haruna",
                        Avatar = "https://i.imgur.com/7GC7FvJ.png"
                    },
                    new ()
                    {
                        Id = 6,
                        Name = "Yamato",
                        Avatar = "https://i.imgur.com/8748bUL.png"
                    },
                    new ()
                    {
                        Id = 7,
                        Name = "Akagi",
                        Avatar = "https://i.imgur.com/VLsezdF.png"
                    },
                    new ()
                    {
                        Id = 8,
                        Name = "Kaga",
                        Avatar = "https://i.imgur.com/eyt9k8E.png"
                    },
                    new ()
                    {
                        Id = 9,
                        Name = "Zero Two",
                        Avatar = "https://i.imgur.com/4XYg6ch.png"
                    },
                    new ()
                    {
                        Id = 10,
                        Name = "Echidna",
                        Avatar = "https://i.imgur.com/Nl6WsbP.png"
                    },
                    new ()
                    {
                        Id = 11,
                        Name = "Emilia",
                        Avatar = "https://i.imgur.com/kF9b4SJ.png"
                    },
                    new ()
                    {
                        Id = 12,
                        Name = "Rem",
                        Avatar = "https://i.imgur.com/y3bb8Sk.png"
                    },
                    new ()
                    {
                        Id = 13,
                        Name = "Ram",
                        Avatar = "https://i.imgur.com/5CcdVBE.png"
                    },
                    new ()
                    {
                        Id = 14,
                        Name = "Gura",
                        Avatar = "https://i.imgur.com/0VYBYEg.png"
                    },
                    new ()
                    {
                        Id = 15,
                        Name = "Shiki",
                        Avatar = "https://i.imgur.com/rYa5iYc.png"
                    },
                    new ()
                    {
                        Id = 16,
                        Name = "Chika",
                        Avatar = "https://i.imgur.com/PT8SsVB.png"
                    },
                    new ()
                    {
                        Id = 17,
                        Name = "Sora",
                        Avatar = "https://i.imgur.com/5xR0ImK.png"
                    },
                    new ()
                    {
                        Id = 18,
                        Name = "Nobuna",
                        Avatar = "https://i.imgur.com/U0NlfJd.png"
                    },
                    new ()
                    {
                        Id = 19,
                        Name = "Akame",
                        Avatar = "https://i.imgur.com/CI9Osi5.png"
                    },
                    new ()
                    {
                        Id = 20,
                        Name = "Shiina",
                        Avatar = "https://i.imgur.com/GhSG97V.png"
                    },
                    new ()
                    {
                        Id = 21,
                        Name = "Bocchi",
                        Avatar = "https://i.imgur.com/VyJf95i.png"
                    },
                    new ()
                    {
                        Id = 22,
                        Name = "Enterprise",
                        Avatar = "https://i.imgur.com/bv5ao8Z.png"
                    },
                    new ()
                    {
                        Id = 23,
                        Name = "Chocola",
                        Avatar = "https://i.imgur.com/HoNwKi9.png"
                    },
                    new ()
                    {
                        Id = 24,
                        Name = "Vanilla",
                        Avatar = "https://i.imgur.com/aijxHla.png"
                    },
                    new ()
                    {
                        Id = 25,
                        Name = "Shiro",
                        Avatar = "https://i.imgur.com/Wxhd5WY.png"
                    }
                });
            });
        }
    }
}