using System;
using System.Collections.Generic;
using Hanekawa.Database.Entities;
using Hanekawa.Database.Tables.Account.Achievement;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Database
{
    public partial class DbService
    {
        // Achievements
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<AchievementUnlocked> AchievementUnlocks { get; set; }
        
        private static void AchievementBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Achievement>(x =>
            {
                x.HasKey(e => e.AchievementId);
                x.Property(e => e.AchievementId).ValueGeneratedOnAdd();
                x.HasData(new List<Achievement>
                {
                    new ()
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 5",
                        Description = "Reach Server Level 5",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 5,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Normal,
                        Unlocked = null
                    },
                    new ()
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 10",
                        Description = "Reach Server Level 10",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 10,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Normal,
                        Unlocked = null
                    },
                    new ()
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 20",
                        Description = "Reach Server Level 20",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 20,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Normal,
                        Unlocked = null
                    },
                    new ()
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 30",
                        Description = "Reach Server Level 30",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 30,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Normal,
                        Unlocked = null
                    },
                    new ()
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 40",
                        Description = "Reach Server Level 40",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 40,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Normal,
                        Unlocked = null
                    },
                    new ()
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 50",
                        Description = "Reach Server Level 50",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 50,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Rare,
                        Unlocked = null
                    },
                    new ()
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 60",
                        Description = "Reach Server Level 60",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 60,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Rare,
                        Unlocked = null
                    },
                    new ()
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Reach Server Level 70",
                        Description = "Reach Server Level 70",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 70,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Epic,
                        Unlocked = null
                    },
                    new ()
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 80",
                        Description = "Reach Server Level 80",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 80,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Epic,
                        Unlocked = null
                    },
                    new ()
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 90",
                        Description = "Reach Server Level 90",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 90,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Legendary,
                        Unlocked = null
                    },
                    new ()
                    {
                        AchievementId = Guid.NewGuid(),
                        Name = "Level 100",
                        Description = "Reach Server Level 100",
                        ImageUrl = "",
                        Points = 10,
                        Reward = null,
                        Requirement = 100,
                        Hidden = false,
                        Category = AchievementCategory.Level,
                        Difficulty = AchievementDifficulty.Legendary,
                        Unlocked = null
                    }
                });
            });
            modelBuilder.Entity<AchievementUnlocked>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
                x.HasOne(e => e.Achievement).WithMany(e => e.Unlocked);
            });
        }
    }
}