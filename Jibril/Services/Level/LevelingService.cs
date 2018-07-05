using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Jibril.Extensions;
using Jibril.Services.Automate;
using Jibril.Services.Entities;
using Jibril.Services.Entities.Tables;
using Jibril.Services.Level.Services;

namespace Jibril.Services.Level
{
    public class LevelingService
    {
        private readonly Calculate _calc;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _provider;

        public LevelingService(IServiceProvider provider, DiscordSocketClient discord)
        {
            _client = discord;
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

        private Task GiveRolesBack(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var userdata = await db.GetOrCreateUserData(user);
                    if (userdata.Level <= 2) return;
                    var cfg = await db.GetOrCreateGuildConfig(user.Guild);
                    if (cfg.StackLvlRoles)
                    {
                        var roleCollection = await GetRoleCollection(user);
                        await user.AddRolesAsync(roleCollection);
                        return;
                    }

                    var singleRole = await GetRoleSingle(user);
                    await user.AddRoleAsync(singleRole);
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
                if (!(msg.Channel is IGuildChannel)) return;

                if (!CheckCooldown(msg.Author as SocketGuildUser)) return;
                using (var db = new DbService())
                {
                    ExpMultiplier.TryGetValue(((IGuildChannel)msg.Channel).GuildId, out var multi);
                    var userdata = await db.GetOrCreateUserData(msg.Author);
                    var exp = _calc.GetMessageExp(msg) * multi;
                    var nxtLvl = _calc.GetNextLevelRequirement(userdata.Level);

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
                    Console.WriteLine($"{message.Author.Username} gained {exp} exp and has {userdata.Exp}/{nxtLvl}");
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        private Task VoiceExp(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var userdata = await db.GetOrCreateUserData(user);
                    var oldVc = oldState.VoiceChannel;
                    var newVc = newState.VoiceChannel;
                    if (newVc != null && oldVc == null)
                    {
                        userdata.VoiceExpTime = DateTime.UtcNow;
                        await db.SaveChangesAsync();
                        return;
                    }

                    if (oldVc == null || newVc != null) return;
                    ExpMultiplier.TryGetValue(oldState.VoiceChannel.Guild.Id, out var multi);
                    var exp = _calc.GetVoiceExp(userdata.VoiceExpTime) * multi;
                    var nxtLvl = _calc.GetNextLevelRequirement(userdata.Level);

                    userdata.TotalExp = userdata.TotalExp + exp;
                    userdata.Credit = userdata.Credit + _calc.GetMessageCredit();

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
            });
            return Task.CompletedTask;
        }

        private async Task NewLevelManager(Account userdata, IGuildUser user, DbService db)
        {
            var role = await GetLevelUpRole(userdata.Level, user);
            if (role == null) return;
            var cfg = await db.GetOrCreateGuildConfig(user.Guild as SocketGuild);
            if (!cfg.StackLvlRoles) await RemoveLevelRoles(user);
            await user.AddRoleAsync(role);
        }

        private async Task<IRole> GetRoleSingle(IGuildUser user)
        {
            using (var db = new DbService())
            {
                var dbUser = await db.GetOrCreateUserData(user);
                ulong roleid = 0;
                foreach (var x in db.LevelRewards)
                    if (dbUser.Level >= x.Level)
                        roleid = x.Role;

                return roleid == 0 ? null : _client.GetGuild(user.GuildId).GetRole(roleid);
            }
        }

        private async Task<List<IRole>> GetRoleCollection(IGuildUser user)
        {
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user);
                var roles = Enumerable.Cast<IRole>(from x in db.LevelRewards
                                                   where userdata.Level >= x.Level
                                                   select _client.GetGuild(user.GuildId).GetRole(x.Role)).ToList();

                return roles.Count == 0 ? null : roles;
            }
        }

        private async Task<IRole> GetLevelUpRole(uint level, IGuildUser user)
        {
            using (var db = new DbService())
            {
                var roleid = await db.LevelRewards.FindAsync(level);
                return roleid == null ? null : _client.GetGuild(user.GuildId).GetRole(roleid.Role);
            }
        }

        private async Task RemoveLevelRoles(IGuildUser user)
        {
            using (var db = new DbService())
            {
                foreach (var x in db.LevelRewards)
                {
                    if (x.Stackable) continue;
                    if (user.RoleIds.Equals(x.Role)) await user.RemoveRoleAsync(user.Guild.GetRole(x.Role));
                }
            }
        }

        private bool CheckCooldown(SocketGuildUser usr)
        {
            //TODO Cooldown system
            return true;
        }
    }
}