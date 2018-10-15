using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Account;
using Hanekawa.Addons.Database.Tables.Achievement;
using Hanekawa.Services.Drop;
using Hanekawa.Services.Games.ShipGame;
using Hanekawa.Services.Level;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace Hanekawa.Services.Achievement
{
    public class AchievementManager
    {
        private readonly DiscordSocketClient _client;
        private readonly DropService _dropService;
        private readonly LevelingService _levelingService;
        private readonly ShipGameService _gameService;

        private const int Special = 1;
        private const int Voice = 2;
        private const int Level = 3;
        private const int Drop = 4;
        private const int PvP = 5;
        private const int PvE = 6;
        private const int Fun = 7;

        public AchievementManager(DropService dropService, LevelingService levelingService, ShipGameService gameService,
            DiscordSocketClient client)
        {
            _dropService = dropService;
            _levelingService = levelingService;
            _gameService = gameService;
            _client = client;

            _dropService.DropClaimed += DropClaimed;
            _levelingService.ServerLevel += ServerLevelAchievement;
            _levelingService.GlobalLevel += GlobalLevelAchievement;
            _levelingService.InVoice += AtOnce;
            _levelingService.InVoice += TotalTime;
            _gameService.NpcKill += NpcAchievement;
            _gameService.PvpKill += PvpAchievement;
            _client.MessageReceived += MessageCount;
        }

        private static Task TotalTime(SocketGuildUser user, TimeSpan time)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var achievements = await db.Achievements.Where(x => x.TypeId == Voice).ToListAsync();
                    var progress = await db.GetAchievementProgress(user, Voice);
                    if (progress == null) return;
                    if (achievements == null) return;

                    var totalTime = Convert.ToInt32(time.TotalMinutes);
                    if (achievements.Any(x => x.Requirement == (progress.Count + totalTime) && x.Once == false))
                    {
                        var achieve = achievements.First(x => x.Requirement == (progress.Count + totalTime) && x.Once == false);
                        var data = new AchievementUnlock
                        {
                            AchievementId = achieve.AchievementId,
                            TypeId = Voice,
                            UserId = user.Id
                        };
                        await db.AchievementUnlocks.AddAsync(data);
                        await db.SaveChangesAsync();
                    }

                    progress.Count = progress.Count + totalTime;
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }
        
        private static Task AtOnce(SocketGuildUser user, TimeSpan time)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var achievements = await db.Achievements.Where(x => x.TypeId == Voice).ToListAsync();
                    var progress = await db.GetAchievementProgress(user, Voice);
                    if (progress == null) return;
                    if (achievements == null) return;

                    var totalTime = Convert.ToInt32(time.TotalMinutes);
                    if (achievements.Any(x => x.Requirement == progress.Count + totalTime && x.Once))
                    {
                        var achieve = achievements.First(x => x.Requirement == progress.Count + totalTime && x.Once);
                        var unlockCheck = await db.AchievementUnlocks.FindAsync(achieve.AchievementId, user.Id);
                        if (unlockCheck != null) return;
                        var data = new AchievementUnlock
                        {
                            AchievementId = achieve.AchievementId,
                            TypeId = Voice,
                            UserId = user.Id
                        };
                        await db.AchievementUnlocks.AddAsync(data);
                        await db.SaveChangesAsync();
                    }
                }
            });
            return Task.CompletedTask;
        }
        
        private static Task ServerLevelAchievement(IGuildUser user, Account userData)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var achievements = await db.Achievements.Where(x => x.TypeId == Level).ToListAsync();
                    if (achievements == null) return;

                    if (achievements.Any(x => x.Requirement == userData.Level && x.Once == false))
                    {
                        var achieve = achievements.First(x => x.Requirement == userData.Level && x.Once == false);
                        var data = new AchievementUnlock
                        {
                            AchievementId = achieve.AchievementId,
                            TypeId = Level,
                            UserId = user.Id
                        };
                        await db.AchievementUnlocks.AddAsync(data);
                        await db.SaveChangesAsync();
                    }
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        private static Task GlobalLevelAchievement(IGuildUser user, AccountGlobal userData)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var achievements = await db.Achievements.Where(x => x.TypeId == Level && x.Global).ToListAsync();
                    if (achievements == null) return;

                    if (achievements.Any(x => x.Requirement == userData.Level && x.Once == false && x.Global))
                    {
                        var achieve = achievements.First(x => x.Requirement == userData.Level && x.Once == false && x.Global);
                        var data = new AchievementUnlock
                        {
                            AchievementId = achieve.AchievementId,
                            TypeId = Level,
                            UserId = user.Id
                        };
                        await db.AchievementUnlocks.AddAsync(data);
                        await db.SaveChangesAsync();
                    }
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask; ;
        }

        private static Task DropClaimed(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var achievements = await db.Achievements.Where(x => x.TypeId == Drop).ToListAsync();
                    var progress = await db.GetAchievementProgress(user, Drop);
                    if (progress == null) return;
                    if (achievements == null) return;

                    if (achievements.Any(x => x.Requirement == progress.Count + 1 && x.Once == false))
                    {
                        var achieve = achievements.First(x => x.Requirement == progress.Count + 1 && x.Once == false);
                        var data = new AchievementUnlock
                        {
                            AchievementId = achieve.AchievementId,
                            TypeId = Drop,
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
                    var achievements = await db.Achievements.Where(x => x.TypeId == PvP).ToListAsync();
                    var progress = await db.GetAchievementProgress(userid, PvP);
                    if (progress == null) return;
                    if (achievements == null) return;

                    if (achievements.Any(x => x.Requirement == progress.Count + 1 && x.Once == false))
                    {
                        var achieve = achievements.First(x => x.Requirement == progress.Count + 1 && x.Once == false);
                        var data = new AchievementUnlock
                        {
                            AchievementId = achieve.AchievementId,
                            TypeId = PvP,
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
                    var achievements = await db.Achievements.Where(x => x.TypeId == PvE).ToListAsync();
                    var progress = await db.GetAchievementProgress(userid, PvE);
                    if (progress == null) return;
                    if (achievements == null) return;

                    if (achievements.Any(x => x.Requirement == progress.Count + 1 && x.Once == false))
                    {
                        var achieve = achievements.First(x => x.Requirement == progress.Count + 1 && x.Once == false);
                        var data = new AchievementUnlock
                        {
                            AchievementId = achieve.AchievementId,
                            TypeId = PvE,
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

        private static Task MessageCount(SocketMessage message)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    if (message.Author.IsBot) return;
                    if (!(message is SocketUserMessage msg)) return;
                    if (message.Source != MessageSource.User) return;
                    if (!(msg.Author is SocketGuildUser user)) return;
                    var achievements = await db.Achievements.Where(x => x.TypeId == Fun).ToListAsync();
                    if (achievements == null) return;

                    if (achievements.Any(x => x.Requirement == msg.Content.Length && x.Once))
                    {
                        var achieve = achievements.First(x => x.Requirement == msg.Content.Length && x.Once);
                        var data = new AchievementUnlock
                        {
                            AchievementId = achieve.AchievementId,
                            TypeId = Fun,
                            UserId = user.Id
                        };
                        await db.AchievementUnlocks.AddAsync(data);
                        await db.SaveChangesAsync();
                    }
                }
            });
            return Task.CompletedTask;
        }
    }
}