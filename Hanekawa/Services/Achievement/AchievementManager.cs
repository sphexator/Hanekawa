using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Account;
using Hanekawa.Addons.Database.Tables.Achievement;
using Hanekawa.Services.Drop;
using Hanekawa.Services.Games.ShipGame;
using Hanekawa.Services.Level;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Services.Achievement
{
    public class AchievementManager
    {
        private readonly DropService _dropService;
        private readonly LevelingService _levelingService;
        private readonly ShipGameService _gameService;

        public AchievementManager(DropService dropService, LevelingService levelingService, ShipGameService gameService)
        {
            _dropService = dropService;
            _levelingService = levelingService;
            _gameService = gameService;

            _dropService.DropClaimed += DropClaimed;
            _levelingService.Level += LevelAchievement;
            _levelingService.InVoice += InVoiceAchievement;
            _gameService.NpcKill += NpcAchievement;
            _gameService.PvpKill += PvpAchievement;
        }

        private static Task InVoiceAchievement(SocketGuildUser user, TimeSpan time)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var achievements = await db.Achievements.Where(x => x.TypeId == 1).ToListAsync();
                    var progress = await db.GetAchievementProgress(user, 1);

                    if (achievements.Any(x => x.Requirement == progress.Count + 1))
                    {
                        var achieve = achievements.First(x => x.Requirement == progress.Count + 1);
                        var data = new AchievementUnlock
                        {
                            AchievementId = achieve.AchievementId,
                            TypeId = 1,
                            UserId = user.Id
                        };
                        await db.AchievementUnlocks.AddAsync(data);
                        await db.SaveChangesAsync();
                    }

                    progress.Count = progress.Count + 1;
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        private static Task LevelAchievement(SocketGuildUser user, Account userData)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var achievements = await db.Achievements.Where(x => x.TypeId == 2).ToListAsync();
                    var progress = await db.GetAchievementProgress(user, 2);

                    if (achievements.Any(x => x.Requirement == progress.Count + 1))
                    {
                        var achieve = achievements.First(x => x.Requirement == progress.Count + 1);
                        var data = new AchievementUnlock
                        {
                            AchievementId = achieve.AchievementId,
                            TypeId = 2,
                            UserId = user.Id
                        };
                        await db.AchievementUnlocks.AddAsync(data);
                        await db.SaveChangesAsync();
                    }

                    progress.Count = progress.Count + 1;
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        private static Task DropClaimed(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var achievements = await db.Achievements.Where(x => x.TypeId == 3).ToListAsync();
                    var progress = await db.GetAchievementProgress(user, 3);

                    if (achievements.Any(x => x.Requirement == progress.Count + 1))
                    {
                        var achieve = achievements.First(x => x.Requirement == progress.Count + 1);
                        var data = new AchievementUnlock
                        {
                            AchievementId = achieve.AchievementId,
                            TypeId = 3,
                            UserId = user.Id
                        };
                        await db.AchievementUnlocks.AddAsync(data);
                        await db.SaveChangesAsync();
                    }

                    progress.Count = progress.Count + 1;
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        private static Task PvpAchievement(ulong userid)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var achievements = await db.Achievements.Where(x => x.TypeId == 4).ToListAsync();
                    var progress = await db.GetAchievementProgress(userid, 4);

                    if (achievements.Any(x => x.Requirement == progress.Count + 1))
                    {
                        var achieve = achievements.First(x => x.Requirement == progress.Count + 1);
                        var data = new AchievementUnlock
                        {
                            AchievementId = achieve.AchievementId,
                            TypeId = 4,
                            UserId = userid
                        };
                        await db.AchievementUnlocks.AddAsync(data);
                        await db.SaveChangesAsync();
                    }

                    progress.Count = progress.Count + 1;
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        private static Task NpcAchievement(ulong userid)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var achievements = await db.Achievements.Where(x => x.TypeId == 4).ToListAsync();
                    var progress = await db.GetAchievementProgress(userid, 4);
                    if (progress == null) return;
                    if (achievements == null) return;
                    if (achievements.Any(x => x.Requirement == progress.Count + 1))
                    {
                        var achieve = achievements.First(x => x.Requirement == progress.Count + 1);
                        var data = new AchievementUnlock
                        {
                            AchievementId = achieve.AchievementId,
                            TypeId = 4,
                            UserId = userid
                        };
                        await db.AchievementUnlocks.AddAsync(data);
                        await db.SaveChangesAsync();
                    }

                    progress.Count = progress.Count + 1;
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }
    }
}