using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Account;
using Hanekawa.Addons.Database.Tables.Achievement;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Services.Drop;
using Hanekawa.Services.Games.ShipGame;
using Hanekawa.Services.Level;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Services.Achievement
{
    public class AchievementManager : IHanaService, IRequiredService
    {
        private const int Special = 1;
        private const int Voice = 2;
        private const int Level = 3;
        private const int Drop = 4;
        private const int PvP = 5;
        private const int PvE = 6;
        private const int Fun = 7;
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;
        private readonly DropService _dropService;
        private readonly ShipGameService _gameService;
        private readonly LevelingService _levelingService;

        public AchievementManager(DropService dropService, LevelingService levelingService, ShipGameService gameService,
            DiscordSocketClient client, DbService dbService)
        {
            _dropService = dropService;
            _levelingService = levelingService;
            _gameService = gameService;
            _client = client;
            _db = dbService;

            _dropService.DropClaimed += DropClaimed;
            _levelingService.ServerLevel += ServerLevelAchievement;
            _levelingService.GlobalLevel += GlobalLevelAchievement;
            _levelingService.InVoice += AtOnce;
            _levelingService.InVoice += TotalTime;
            _gameService.NpcKill += NpcAchievement;
            _gameService.PvpKill += PvpAchievement;
            _client.MessageReceived += MessageCount;
            Console.WriteLine("Achievement manager service loaded");
        }

        private Task TotalTime(SocketGuildUser user, TimeSpan time)
        {
            var _ = Task.Run(async () =>
            {
                var achievements = await _db.Achievements.Where(x => x.TypeId == Voice && x.AchievementNameId == 15)
                    .ToListAsync();
                var progress = await _db.GetAchievementProgress(user, Voice);
                if (progress == null) return;

                if (achievements == null || achievements.Count == 0) return;

                var totalTime = Convert.ToInt32(time.TotalMinutes);
                if (achievements.Any(x => x.Requirement == progress.Count + totalTime && x.Once == false))
                {
                    var achieve = achievements.First(x =>
                        x.Requirement == progress.Count + totalTime && x.Once == false);
                    var data = new AchievementUnlock
                    {
                        AchievementId = achieve.AchievementId,
                        TypeId = Voice,
                        UserId = user.Id,
                        Achievement = achieve
                    };
                    await _db.AchievementUnlocks.AddAsync(data);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    var below = achievements
                        .Where(x => x.Requirement < progress.Count + totalTime && x.Once == false).ToList();
                    if (below.Count != 0)
                    {
                        var unlocked = await _db.AchievementUnlocks.Where(x => x.TypeId == Voice).ToListAsync();
                        foreach (var x in below)
                        {
                            if (unlocked.Any(y => y.AchievementId == x.AchievementId)) continue;

                            var data = new AchievementUnlock
                            {
                                AchievementId = x.AchievementId,
                                TypeId = Level,
                                UserId = user.Id,
                                Achievement = x
                            };
                            await _db.AchievementUnlocks.AddAsync(data);
                        }
                    }
                }

                progress.Count = progress.Count + totalTime;
                await _db.SaveChangesAsync();
            });
            return Task.CompletedTask;
        }

        private Task AtOnce(SocketGuildUser user, TimeSpan time)
        {
            var _ = Task.Run(async () =>
            {
                var achievements = await _db.Achievements.Where(x => x.TypeId == Voice).ToListAsync();
                if (achievements == null || achievements.Count == 0) return;

                var totalTime = Convert.ToInt32(time.TotalMinutes);
                if (achievements.Any(x => x.Requirement == totalTime && x.AchievementNameId == 8))
                {
                    var achieve = achievements.First(x => x.Requirement == totalTime && x.Once);
                    var unlockCheck = await _db.AchievementUnlocks.FirstOrDefaultAsync(x =>
                        x.AchievementId == achieve.AchievementId && x.UserId == user.Id);
                    if (unlockCheck != null) return;

                    var data = new AchievementUnlock
                    {
                        AchievementId = achieve.AchievementId,
                        TypeId = Voice,
                        UserId = user.Id,
                        Achievement = achieve
                    };
                    await _db.AchievementUnlocks.AddAsync(data);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    var below = achievements.Where(x => x.Requirement < totalTime).ToList();
                    var unlock = await _db.AchievementUnlocks.Where(x => x.UserId == user.Id && x.TypeId == Voice)
                        .ToListAsync();
                    foreach (var x in below)
                    {
                        if (unlock.Any(y => y.AchievementId == x.AchievementId)) continue;

                        var data = new AchievementUnlock
                        {
                            AchievementId = x.AchievementId,
                            TypeId = Level,
                            UserId = user.Id,
                            Achievement = x
                        };
                        await _db.AchievementUnlocks.AddAsync(data);
                    }
                }
            });
            return Task.CompletedTask;
        }

        private Task ServerLevelAchievement(IGuildUser user, Account userData)
        {
            var _ = Task.Run(async () =>
            {
                var achievements = await _db.Achievements.Where(x => x.TypeId == Level && !x.Once && !x.Global)
                    .ToListAsync();
                if (achievements == null || achievements.Count == 0) return;

                if (achievements.Any(x => x.Requirement == userData.Level))
                {
                    var achieve = achievements.First(x => x.Requirement == userData.Level);
                    var data = new AchievementUnlock
                    {
                        AchievementId = achieve.AchievementId,
                        TypeId = Level,
                        UserId = user.Id,
                        Achievement = achieve
                    };
                    await _db.AchievementUnlocks.AddAsync(data);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    var belowAchieves = achievements
                        .Where(x => x.Requirement < userData.Level).ToList();
                    if (belowAchieves.Count > 0)
                    {
                        var unlocked = await _db.AchievementUnlocks.Where(x => x.UserId == user.Id).ToListAsync();
                        foreach (var x in belowAchieves)
                        {
                            if (unlocked.Any(y => y.AchievementId == x.AchievementId)) continue;

                            var data = new AchievementUnlock
                            {
                                AchievementId = x.AchievementId,
                                TypeId = Level,
                                UserId = user.Id,
                                Achievement = x
                            };
                            await _db.AchievementUnlocks.AddAsync(data);
                        }
                    }
                }

                await _db.SaveChangesAsync();
            });
            return Task.CompletedTask;
        }

        private Task GlobalLevelAchievement(IGuildUser user, AccountGlobal userData)
        {
            var _ = Task.Run(async () =>
            {
                var achievements = await _db.Achievements
                    .Where(x => x.TypeId == Level && x.Once == false && x.Global).ToListAsync();
                if (achievements == null || achievements.Count == 0) return;

                if (achievements.Any(x => x.Requirement == userData.Level))
                {
                    var achieve = achievements.First(x => x.Requirement == userData.Level);
                    var data = new AchievementUnlock
                    {
                        AchievementId = achieve.AchievementId,
                        TypeId = Level,
                        UserId = user.Id,
                        Achievement = achieve
                    };
                    await _db.AchievementUnlocks.AddAsync(data);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    var belowAchieves = achievements.Where(x => x.Requirement < (int) userData.Level).ToList();
                    if (belowAchieves.Count > 0)
                    {
                        var unlocked = await _db.AchievementUnlocks.Where(x => x.UserId == user.Id).ToListAsync();
                        foreach (var x in belowAchieves)
                        {
                            if (unlocked.Any(y => y.AchievementId == x.AchievementId)) continue;

                            var data = new AchievementUnlock
                            {
                                AchievementId = x.AchievementId,
                                TypeId = Level,
                                UserId = user.Id,
                                Achievement = x
                            };
                            await _db.AchievementUnlocks.AddAsync(data);
                        }
                    }
                }

                await _db.SaveChangesAsync();
            });
            return Task.CompletedTask;
        }

        private Task DropClaimed(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                var achievements = await _db.Achievements.Where(x => x.TypeId == Drop).ToListAsync();
                var progress = await _db.GetAchievementProgress(user, Drop);
                if (progress == null) return;

                if (achievements == null) return;

                if (achievements.Any(x => x.Requirement == progress.Count + 1 && x.Once == false))
                {
                    var achieve = achievements.First(x => x.Requirement == progress.Count + 1 && x.Once == false);
                    var data = new AchievementUnlock
                    {
                        AchievementId = achieve.AchievementId,
                        TypeId = Drop,
                        UserId = user.Id,
                        Achievement = achieve
                    };
                    await _db.AchievementUnlocks.AddAsync(data);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    var below = achievements.Where(x => x.Requirement < progress.Count + 1 && !x.Once).ToList();
                    if (below.Count != 0)
                    {
                        var unlocked = await _db.AchievementUnlocks.Where(x => x.UserId == user.Id).ToListAsync();
                        foreach (var x in below)
                        {
                            if (unlocked.Any(y => y.AchievementId == x.AchievementId)) continue;

                            var data = new AchievementUnlock
                            {
                                AchievementId = x.AchievementId,
                                TypeId = Level,
                                UserId = user.Id,
                                Achievement = x
                            };
                            await _db.AchievementUnlocks.AddAsync(data);
                        }
                    }
                }

                progress.Count = progress.Count + 1;
                await _db.SaveChangesAsync();
            });
            return Task.CompletedTask;
        }

        private Task PvpAchievement(ulong userid)
        {
            var _ = Task.Run(async () =>
            {
                var achievements = await _db.Achievements.Where(x => x.TypeId == PvP && !x.Once).ToListAsync();
                var progress = await _db.GetAchievementProgress(userid, PvP);
                if (progress == null) return;

                if (achievements == null) return;

                if (achievements.Any(x => x.Requirement == progress.Count + 1 && !x.Once))
                {
                    var achieve = achievements.First(x => x.Requirement == progress.Count + 1);
                    var data = new AchievementUnlock
                    {
                        AchievementId = achieve.AchievementId,
                        TypeId = PvP,
                        UserId = userid,
                        Achievement = achieve
                    };
                    await _db.AchievementUnlocks.AddAsync(data);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    var below = achievements.Where(x => x.Requirement < progress.Count + 1).ToList();
                    if (below.Count != 0)
                    {
                        var unlocked = await _db.AchievementUnlocks.Where(x => x.UserId == userid).ToListAsync();
                        foreach (var x in below)
                        {
                            if (unlocked.Any(y => y.AchievementId == x.AchievementId)) continue;

                            var data = new AchievementUnlock
                            {
                                AchievementId = x.AchievementId,
                                TypeId = Level,
                                UserId = userid,
                                Achievement = x
                            };
                            await _db.AchievementUnlocks.AddAsync(data);
                        }
                    }
                }

                progress.Count = progress.Count + 1;
                await _db.SaveChangesAsync();
            });
            return Task.CompletedTask;
        }

        private Task NpcAchievement(ulong userid)
        {
            var _ = Task.Run(async () =>
            {
                var achievements =
                    await _db.Achievements.Where(x => x.TypeId == PvE && x.Once == false).ToListAsync();
                var progress = await _db.GetAchievementProgress(userid, PvE);
                if (progress == null) return;

                if (achievements == null) return;

                if (achievements.Any(x => x.Requirement == progress.Count + 1))
                {
                    var achieve = achievements.First(x => x.Requirement == progress.Count + 1);
                    var data = new AchievementUnlock
                    {
                        AchievementId = achieve.AchievementId,
                        TypeId = PvE,
                        UserId = userid,
                        Achievement = achieve
                    };
                    await _db.AchievementUnlocks.AddAsync(data);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    var below = achievements.Where(x => x.Requirement < progress.Count + 1).ToList();
                    if (below.Count != 0)
                    {
                        var unlocked = await _db.AchievementUnlocks.Where(x => x.UserId == userid).ToListAsync();
                        foreach (var x in below)
                        {
                            if (unlocked.Any(y => y.AchievementId == x.AchievementId)) continue;

                            var data = new AchievementUnlock
                            {
                                AchievementId = x.AchievementId,
                                TypeId = Level,
                                UserId = userid,
                                Achievement = x
                            };
                            await _db.AchievementUnlocks.AddAsync(data);
                        }
                    }
                }

                progress.Count = progress.Count + 1;
                await _db.SaveChangesAsync();
            });
            return Task.CompletedTask;
        }

        private Task MessageCount(SocketMessage message)
        {
            var _ = Task.Run(async () =>
            {
                if (message.Author.IsBot) return;
                if (!(message is SocketUserMessage msg)) return;
                if (message.Source != MessageSource.User) return;
                if (!(msg.Author is SocketGuildUser user)) return;


                var achievements = await _db.Achievements.Where(x => x.TypeId == Fun).ToListAsync();
                if (achievements == null) return;

                if (achievements.Any(x => x.Requirement == msg.Content.Length && x.Once))
                {
                    var achieve = achievements.First(x => x.Requirement == msg.Content.Length && x.Once);
                    var data = new AchievementUnlock
                    {
                        AchievementId = achieve.AchievementId,
                        TypeId = Fun,
                        UserId = user.Id,
                        Achievement = achieve
                    };
                    await _db.AchievementUnlocks.AddAsync(data);
                    await _db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }
    }
}