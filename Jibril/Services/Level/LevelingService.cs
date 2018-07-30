using Discord;
using Discord.WebSocket;
using Humanizer;
using Jibril.Extensions;
using Jibril.Services.Entities;
using Jibril.Services.Entities.Tables;
using Jibril.Services.Level.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Jibril.Services.Level
{
    public class LevelingService
    {
        private readonly Calculate _calc;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _provider;

        public LevelingService(IServiceProvider provider, DiscordSocketClient discord, Calculate calc)
        {
            _client = discord;
            _calc = calc;
            _provider = provider;

            _client.MessageReceived += MessageExp;
            _client.UserVoiceStateUpdated += VoiceExp;
            _client.UserJoined += GiveRolesBack;

            using (var db = new DbService())
            {
                foreach (var x in db.GuildConfigs) ExpMultiplier.TryAdd(x.GuildId, x.ExpMultiplier);
            }
        }

        private ConcurrentDictionary<ulong, uint> ExpMultiplier { get; }
            = new ConcurrentDictionary<ulong, uint>();
        private ConcurrentDictionary<ulong, Timer> ExpEvent { get; }
            = new ConcurrentDictionary<ulong, Timer>();
        private ConcurrentDictionary<ulong, Timer> ExpEventMessage { get; }
            = new ConcurrentDictionary<ulong, Timer>();
        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DateTime>> ServerExpCooldown { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DateTime>>();

        public async Task AddExpMultiplierAsync(IGuild guild, uint multiplier, TimeSpan after, bool announce = false, SocketTextChannel fallbackChannel = null)
        {
            ExpMultiplier.AddOrUpdate(guild.Id, multiplier, (key, old) => old = multiplier);
            StartExpEvent(guild.Id, after);
            if (announce)
            {
                await AnnounceExpEvent(guild, multiplier, after, fallbackChannel);
            }
        }

        private void StartExpEvent(ulong guildid, TimeSpan after)
        {
            var toAdd = new Timer(_ =>
            {
                ExpMultiplier.AddOrUpdate(guildid, 1, (key, old) => old = 1);

            }, null, after, Timeout.InfiniteTimeSpan);
            ExpEvent.AddOrUpdate(guildid, (key) => toAdd, (key, old) =>
            {
                old.Change(Timeout.Infinite, Timeout.Infinite);
                return toAdd;
            });
        }

        private async Task AnnounceExpEvent(IGuild guild, uint multiplier, TimeSpan after, SocketTextChannel fallbackChannel)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(guild as SocketGuild);
                if (cfg.EventChannel.HasValue)
                {
                    var channel = await guild.GetTextChannelAsync(cfg.EventChannel.Value);
                    var msg = await channel.SendEmbedAsync(new EmbedBuilder().Reply($"A {multiplier}x exp event has started!\n" +
                                                                          $"Duration: {after.Humanize()} ({after} minutes)"));
                    var toAdd = new Timer(_ =>
                    {
                        var upd = msg.Embeds.First().ToEmbedBuilder();
                        upd.Color = Color.Red;
                        msg.ModifyAsync(x => x.Embed = upd.Build());
                    }, null, after, Timeout.InfiniteTimeSpan);
                    ExpEventMessage.AddOrUpdate(guild.Id, (key) => toAdd, (key, old) =>
                    {
                        old.Change(Timeout.Infinite, Timeout.Infinite);
                        return toAdd;
                    });
                }
                else
                    await fallbackChannel.SendEmbedAsync(new EmbedBuilder().Reply($"No event channel has been setup.", Color.Red.RawValue));
            }
        }

        private static Task GiveRolesBack(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var userdata = await db.GetOrCreateUserData(user);
                    if (userdata.Level < 2) return;
                    var cfg = await db.GetOrCreateGuildConfig(user.Guild);
                    if (cfg.StackLvlRoles)
                    {
                        var roleCollection = await GetRoleCollection(user, db, userdata);
                        await user.AddRolesAsync(roleCollection);
                        return;
                    }

                    var singleRole = await GetRoleSingle(user, db, userdata);
                    await user.AddRolesAsync(singleRole);
                }
            });
            return Task.CompletedTask;
        }

        private Task MessageExp(SocketMessage message)
        {
            var _ = Task.Run(async () =>
            {
                if (!(message is SocketUserMessage msg)) return;
                if (msg.Source != MessageSource.User) return;
                if (msg.Author.IsBot) return;
                if (!(msg.Channel is IGuildChannel)) return;

                if (!CheckCooldown(msg.Author as SocketGuildUser)) return;
                using (var db = new DbService())
                {
                    ExpMultiplier.TryGetValue(((IGuildChannel)msg.Channel).GuildId, out var multi);
                    var userdata = await db.GetOrCreateUserData(msg.Author as SocketGuildUser);
                    var exp = _calc.GetMessageExp(msg) * multi;
                    var nxtLvl = _calc.GetNextLevelRequirement(userdata.Level);

                    userdata.LastMessage = DateTime.UtcNow;
                    if (!userdata.FirstMessage.HasValue) userdata.FirstMessage = DateTime.UtcNow;

                    userdata.TotalExp = userdata.TotalExp + exp;
                    userdata.Credit = userdata.Credit + _calc.GetMessageCredit();

                    if (userdata.Exp + exp >= nxtLvl)
                    {
                        userdata.Level = userdata.Level + 1;
                        userdata.Exp = userdata.Exp + exp - nxtLvl;
                        await NewLevelManager(userdata, msg.Author as IGuildUser, db);
                    }
                    else
                    {
                        userdata.Exp = userdata.Exp + exp;
                    }
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        private Task VoiceExp(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    using (var db = new DbService())
                    {
                        var userdata = await db.GetOrCreateUserData(user as SocketGuildUser);
                        var oldVc = oldState.VoiceChannel;
                        var newVc = newState.VoiceChannel;
                        if (newVc != null && oldVc == null)
                        {
                            userdata.VoiceExpTime = DateTime.UtcNow;
                            await db.SaveChangesAsync();
                            return;
                        }

                        if (oldVc == null || newVc != null) return;
                        var multi = ExpMultiplier.GetOrAdd(oldState.VoiceChannel.Guild.Id, 1);
                        var exp = _calc.GetVoiceExp(userdata.VoiceExpTime) * multi;
                        var nxtLvl = _calc.GetNextLevelRequirement(userdata.Level);

                        userdata.TotalExp = userdata.TotalExp + exp;
                        userdata.Credit = userdata.Credit + _calc.GetMessageCredit();
                        userdata.StatVoiceTime = userdata.StatVoiceTime + (DateTime.UtcNow - userdata.VoiceExpTime);
                        userdata.Sessions = userdata.Sessions + 1;

                        if (userdata.Exp + exp >= nxtLvl)
                        {
                            userdata.Level = userdata.Level + 1;
                            userdata.Exp = userdata.Exp + exp - nxtLvl;
                            await NewLevelManager(userdata, user as IGuildUser, db);
                        }
                        else
                        {
                            userdata.Exp = userdata.Exp + exp;
                        }
                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            return Task.CompletedTask;
        }

        private async Task NewLevelManager(Account userdata, IGuildUser user, DbService db)
        {
            var roles = await db.LevelRewards.Where(x => x.GuildId == user.GuildId).ToListAsync();
            var role = GetLevelUpRole(userdata.Level, user, roles);
            if (role == null) return;
            var cfg = await db.GetOrCreateGuildConfig(user.Guild as SocketGuild);
            if (!cfg.StackLvlRoles) await RemoveLevelRoles(user, roles);
            await user.AddRoleAsync(role);
        }

        private static async Task<List<IRole>> GetRoleSingle(IGuildUser user, DbService db, Account userdata)
        {
            var roles = new List<IRole>();
            ulong role = 0;

            foreach (var x in await db.LevelRewards.Where(x => x.GuildId == user.GuildId).ToListAsync())
            {
                if (userdata.Level >= x.Level && x.Stackable)
                {
                    roles.Add(user.Guild.GetRole(x.Role));
                }

                if (userdata.Level >= x.Level) role = x.Role;
            }
            if(role != 0) roles.Add(user.Guild.GetRole(role));
            return roles;
        }

        private static async Task<List<IRole>> GetRoleCollection(IGuildUser user, DbService db, Account userdata)
        {
            var roles = new List<IRole>();

            foreach (var x in await db.LevelRewards.Where(x => x.GuildId == user.GuildId).ToListAsync())
            {
                if (userdata.Level >= x.Level)
                {
                    roles.Add(user.Guild.GetRole(x.Role));
                }
            }
            return roles;
        }

        private IRole GetLevelUpRole(uint level, IGuildUser user, IEnumerable<LevelReward> rolesRewards)
        {
            var roleid = rolesRewards.FirstOrDefault(x => x.Level == level);
            return roleid == null ? null : _client.GetGuild(user.GuildId).GetRole(roleid.Role);
        }

        private static async Task RemoveLevelRoles(IGuildUser user, IEnumerable<LevelReward> rolesRewards)
        {
            foreach (var x in rolesRewards)
            {
                if (x.Stackable) continue;
                if (user.RoleIds.Contains(x.Role)) await user.RemoveRoleAsync(user.Guild.GetRole(x.Role));
            }
        }

        private bool CheckCooldown(IGuildUser usr)
        {
            var check = ServerExpCooldown.TryGetValue(usr.GuildId, out var cds);
            if (!check)
            {
                ServerExpCooldown.TryAdd(usr.GuildId, new ConcurrentDictionary<ulong, DateTime>());
                ServerExpCooldown.TryGetValue(usr.GuildId, out cds);
                cds.TryAdd(usr.Id, DateTime.UtcNow);
                return true;
            }

            var userCheck = cds.TryGetValue(usr.Id, out var cd);
            if (!userCheck)
            {
                cds.TryAdd(usr.Id, DateTime.UtcNow);
                return true;
            }

            if (!((DateTime.UtcNow - cd).TotalSeconds >= 60)) return false;
            cds.AddOrUpdate(usr.Id, DateTime.UtcNow, (key, old) => old = DateTime.UtcNow);
            return true;
        }
    }
}