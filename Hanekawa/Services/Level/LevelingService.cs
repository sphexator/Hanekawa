using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Account;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Hanekawa.Events;
using Hanekawa.Extensions;
using Hanekawa.Services.Level.Util;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Services.Level
{
    public class LevelingService
    {
        private readonly Calculate _calc;
        private readonly DiscordSocketClient _client;

        public LevelingService(DiscordSocketClient discord, Calculate calc)
        {
            _client = discord;
            _calc = calc;

            _client.MessageReceived += ServerMessageExpAsync;
            _client.MessageReceived += GlobalMessageExpAsync;
            _client.UserVoiceStateUpdated += VoiceExpAsync;
            _client.UserJoined += GiveRolesBackAsync;

            _client.UserJoined += UserActiveAsync;
            _client.UserLeft += UserDeactiveAsync;

            using (var db = new DbService())
            {
                foreach (var x in db.GuildConfigs)
                {
                    var expEvent = db.LevelExpEvents.Find(x.GuildId);
                    if (expEvent == null)
                    {
                        ExpMultiplier.TryAdd(x.GuildId, x.ExpMultiplier);
                    }
                    else if (expEvent.Time - TimeSpan.FromMinutes(2) <= DateTime.UtcNow)
                    {
                        ExpMultiplier.TryAdd(x.GuildId, x.ExpMultiplier);
                        RemoveFromDatabase(db, x.GuildId);
                    }
                    else
                    {
                        var after = expEvent.Time - DateTime.UtcNow;
                        ExpEventHandler(db, expEvent.GuildId, expEvent.Multiplier, x.ExpMultiplier, expEvent.MessageId,
                            expEvent.ChannelId, after);
                    }
                }
            }
        }

        private ConcurrentDictionary<ulong, uint> ExpMultiplier { get; }
            = new ConcurrentDictionary<ulong, uint>();

        private ConcurrentDictionary<ulong, Timer> ExpEvent { get; }
            = new ConcurrentDictionary<ulong, Timer>();

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DateTime>> ServerExpCooldown { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DateTime>>();

        private ConcurrentDictionary<ulong, DateTime> GlobalExpCooldown { get; }
            = new ConcurrentDictionary<ulong, DateTime>();

        public event AsyncEvent<IGuildUser, Account> ServerLevel;
        public event AsyncEvent<IGuildUser, AccountGlobal> GlobalLevel;
        public event AsyncEvent<SocketGuildUser, TimeSpan> InVoice;

        // Get Commands
        public uint GetServerMultiplier(IGuild guild)
        {
            return ExpMultiplier.GetOrAdd(guild.Id, 1);
        }

        // Exp event handler
        public async Task StartExpEventAsync(IGuild guild, uint multiplier, TimeSpan after, bool announce = false,
            ITextChannel fallbackChannel = null)
        {
            using (var db = new DbService())
            {
                IUserMessage message = null;
                var cfg = await db.GetOrCreateGuildConfig(guild as SocketGuild);
                if (announce) message = await AnnounceExpEventAsync(db, cfg, guild, multiplier, after, fallbackChannel);
                ExpEventHandler(db, guild.Id, multiplier, cfg.ExpMultiplier, message.Id, message.Channel.Id, after);
                await EventAddOrUpdateDatabaseAsync(db, guild.Id, multiplier, message.Id, message.Channel.Id, after);
            }
        }

        private void ExpEventHandler(DbService db, ulong guildId, uint multiplier, uint defaultMult, ulong? messageId,
            ulong? channelId, TimeSpan after)
        {
            ExpMultiplier.AddOrUpdate(guildId, multiplier, (key, old) => old = multiplier);
            var toAdd = new Timer(async _ =>
            {
                try
                {
                    ExpMultiplier.AddOrUpdate(guildId, defaultMult, (key, old) => old = defaultMult);
                    if (messageId != null)
                    {
                        var msg = await _client.GetGuild(guildId).GetTextChannel(channelId.Value)
                            .GetMessageAsync(messageId.Value) as IUserMessage;
                        var upd = msg.Embeds.First().ToEmbedBuilder();
                        upd.Color = Color.Red;
                        await msg.ModifyAsync(x => x.Embed = upd.Build());
                    }

                    RemoveFromDatabase(db, guildId);
                    ExpEvent.Remove(guildId, out var timer);
                }
                catch
                {
                    ExpMultiplier.AddOrUpdate(guildId, defaultMult, (key, old) => old = defaultMult);
                    RemoveFromDatabase(db, guildId);
                }
            }, null, after, Timeout.InfiniteTimeSpan);
            ExpEvent.AddOrUpdate(guildId, key => toAdd, (key, old) =>
            {
                old.Change(Timeout.Infinite, Timeout.Infinite);
                return toAdd;
            });
        }

        private static async Task EventAddOrUpdateDatabaseAsync(DbService db, ulong guildid, uint multiplier,
            ulong? message, ulong? channel, TimeSpan after)
        {
            var check = await db.LevelExpEvents.FindAsync(guildid);
            if (check == null)
            {
                var data = new LevelExpEvent
                {
                    GuildId = guildid,
                    MessageId = message,
                    ChannelId = channel,
                    Multiplier = multiplier,
                    Time = DateTime.UtcNow + after
                };
                await db.LevelExpEvents.AddAsync(data);
                await db.SaveChangesAsync();
            }
            else
            {
                check.ChannelId = channel.Value;
                check.Time = DateTime.UtcNow + after;
                check.Multiplier = multiplier;
                check.MessageId = message.Value;
                await db.SaveChangesAsync();
            }
        }

        private static void RemoveFromDatabase(DbService db, ulong guildid)
        {
            var check = db.LevelExpEvents.FirstOrDefault(x => x.GuildId == guildid);
            if (check == null) return;
            db.LevelExpEvents.Remove(check);
            db.SaveChanges();
        }

        private async Task<IUserMessage> AnnounceExpEventAsync(DbService db, GuildConfig cfg, IGuild guild,
            uint multiplier, TimeSpan after, IMessageChannel fallbackChannel)
        {
            var check = await db.LevelExpEvents.FindAsync(guild.Id);
            if (check == null) return await PostAnnouncementAsync(guild, cfg, multiplier, after, fallbackChannel);
            try
            {
                var msg = await _client.GetGuild(guild.Id).GetTextChannel(check.ChannelId.Value)
                    .GetMessageAsync(check.MessageId.Value);
                if (msg is null) return await PostAnnouncementAsync(guild, cfg, multiplier, after, fallbackChannel);

                await msg.DeleteAsync();
                return await PostAnnouncementAsync(guild, cfg, multiplier, after, fallbackChannel);
            }
            catch
            {
                return await PostAnnouncementAsync(guild, cfg, multiplier, after, fallbackChannel);
            }
        }

        private static async Task<IUserMessage> PostAnnouncementAsync(IGuild guild, GuildConfig cfg, uint multiplier,
            TimeSpan after, IMessageChannel fallbackChannel)
        {
            if (cfg.EventChannel.HasValue)
            {
                var channel = await guild.GetTextChannelAsync(cfg.EventChannel.Value);
                var embed = new EmbedBuilder
                {
                    Color = Color.Purple,
                    Title = "Exp Event",
                    Description = $"A {multiplier}x exp event has started!\n" +
                                  $"Duration: {after.Humanize()} ( {after} )",
                    Timestamp = DateTimeOffset.UtcNow + after,
                    Footer = new EmbedFooterBuilder {Text = "Ends:"}
                };
                var msg = await channel.SendEmbedAsync(embed);
                return msg;
            }

            await fallbackChannel.SendEmbedAsync(new EmbedBuilder().Reply("No event channel has been setup.",
                Color.Red.RawValue));
            return null;
        }

        // Event handlers for exp
        private Task ServerMessageExpAsync(SocketMessage message)
        {
            var _ = Task.Run(async () =>
            {
                if (message.Author.IsBot) return;
                if (!(message is SocketUserMessage msg)) return;
                if (msg.Source != MessageSource.User) return;
                if (!(msg.Channel is ITextChannel channel)) return;
                if (!(msg.Author is SocketGuildUser user)) return;

                if (!CheckServerCooldown(user)) return;

                using (var db = new DbService())
                {
                    ExpMultiplier.TryGetValue(((IGuildChannel) msg.Channel).GuildId, out var multi);
                    var userdata = await db.GetOrCreateUserData(user);
                    var cfg = await db.GetOrCreateGuildConfig(((IGuildChannel) msg.Channel).Guild);
                    var exp = _calc.GetMessageExp(msg) * cfg.ExpMultiplier * multi;
                    var nxtLvl = _calc.GetServerLevelRequirement(userdata.Level);

                    userdata.LastMessage = DateTime.UtcNow;
                    if (!userdata.FirstMessage.HasValue) userdata.FirstMessage = DateTime.UtcNow;

                    userdata.TotalExp = userdata.TotalExp + exp;
                    userdata.Credit = userdata.Credit + _calc.GetMessageCredit();

                    if (userdata.Exp + exp >= nxtLvl)
                    {
                        userdata.Level = userdata.Level + 1;
                        userdata.Exp = userdata.Exp + exp - nxtLvl;
                        await NewLevelManagerAsync(userdata, user, db);
                        ServerLevel(user, userdata);
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

        private Task GlobalMessageExpAsync(SocketMessage message)
        {
            var _ = Task.Run(async () =>
            {
                if (message.Author.IsBot) return;
                if (!(message is SocketUserMessage msg)) return;
                if (msg.Source != MessageSource.User) return;
                if (!(msg.Author is SocketGuildUser user)) return;

                if (!CheckGlobalCooldown(user)) return;

                using (var db = new DbService())
                {
                    var userdata = await db.GetOrCreateGlobalUserData(user);
                    var exp = _calc.GetMessageExp(msg);
                    var nextLevel = _calc.GetGlobalLevelRequirement(userdata.Level);
                    userdata.TotalExp = userdata.TotalExp + exp;
                    userdata.Credit = userdata.Credit + _calc.GetMessageCredit();
                    if (userdata.Exp + exp >= nextLevel)
                    {
                        userdata.Exp = userdata.Exp + exp - nextLevel;
                        userdata.Level = userdata.Level + 1;
                        GlobalLevel(user, userdata);
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

        private Task VoiceExpAsync(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var _ = Task.Run(async () =>
            {
                if (!(user is SocketGuildUser gusr)) return;
                if (user.IsBot) return;
                try
                {
                    using (var db = new DbService())
                    {
                        var userdata = await db.GetOrCreateUserData(gusr);
                        var cfg = await db.GetOrCreateGuildConfig(gusr.Guild);
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
                        var exp = _calc.GetVoiceExp(userdata.VoiceExpTime) * cfg.ExpMultiplier * multi;
                        var nxtLvl = _calc.GetServerLevelRequirement(userdata.Level);

                        userdata.TotalExp = userdata.TotalExp + exp;
                        userdata.Credit = userdata.Credit + _calc.GetVoiceCredit(userdata.VoiceExpTime);
                        userdata.StatVoiceTime = userdata.StatVoiceTime + (DateTime.UtcNow - userdata.VoiceExpTime);
                        userdata.Sessions = userdata.Sessions + 1;

                        if (userdata.Exp + exp >= nxtLvl)
                        {
                            userdata.Level = userdata.Level + 1;
                            userdata.Exp = userdata.Exp + exp - nxtLvl;
                            await NewLevelManagerAsync(userdata, gusr, db);
                        }
                        else
                        {
                            userdata.Exp = userdata.Exp + exp;
                        }

                        await db.SaveChangesAsync();
                        await InVoice?.Invoke(gusr, DateTime.UtcNow - userdata.VoiceExpTime);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            return Task.CompletedTask;
        }

        private static Task UserActiveAsync(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var userdata = await db.Accounts.FindAsync(user.Guild.Id, user.Id);
                    if (userdata == null) return;
                    userdata.Active = true;
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        private static Task UserDeactiveAsync(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var userdata = await db.GetOrCreateUserData(user);
                    userdata.Active = false;
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        // Level manager & roles
        private async Task NewLevelManagerAsync(Account userdata, IGuildUser user, DbService db)
        {
            var roles = await db.LevelRewards.Where(x => x.GuildId == user.GuildId).ToListAsync();
            var role = GetLevelUpRole(userdata.Level, user, roles);
            var cfg = await db.GetOrCreateGuildConfig(user.Guild as SocketGuild);

            if (role == null)
            {
                await RoleCheckAsync(db, user, cfg, userdata);
                return;
            }

            if (!cfg.StackLvlRoles) await RemoveLevelRolesAsync(user, roles);
            await user.AddRoleAsync(role);
        }

        private IRole GetLevelUpRole(uint level, IGuildUser user, IEnumerable<LevelReward> rolesRewards)
        {
            var roleid = rolesRewards.FirstOrDefault(x => x.Level == level);
            return roleid == null ? null : _client.GetGuild(user.GuildId).GetRole(roleid.Role);
        }

        private static async Task RemoveLevelRolesAsync(IGuildUser user, IEnumerable<LevelReward> rolesRewards)
        {
            foreach (var x in rolesRewards)
            {
                if (x.Stackable) continue;
                if (user.RoleIds.Contains(x.Role)) await user.RemoveRoleAsync(user.Guild.GetRole(x.Role));
            }
        }

        private static Task GiveRolesBackAsync(SocketGuildUser user)
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
                        var roleCollection = await GetRoleCollectionAsync(user, db, userdata);
                        await user.AddRolesAsync(roleCollection);
                        return;
                    }

                    var singleRole = await GetRoleSingleAsync(user, db, userdata);
                    await user.AddRolesAsync(singleRole);
                }
            });
            return Task.CompletedTask;
        }

        private static async Task RoleCheckAsync(DbService db, IGuildUser user, GuildConfig cfg, Account userdata)
        {
            var roles = new List<IRole>();
            if (cfg.StackLvlRoles) roles = await GetRoleCollectionAsync(user, db, userdata);
            else roles = await GetRoleSingleAsync(user, db, userdata);
            var missingRoles = new List<IRole>();
            foreach (var x in roles)
                if (!user.RoleIds.Contains(x.Id))
                    missingRoles.Add(x);

            if (missingRoles.Count == 0) return;
            if (missingRoles.Count > 1) await user.AddRolesAsync(missingRoles);
            else await user.AddRoleAsync(missingRoles.FirstOrDefault());
        }

        private static async Task<List<IRole>> GetRoleSingleAsync(IGuildUser user, DbService db, Account userdata)
        {
            var roles = new List<IRole>();
            ulong role = 0;

            foreach (var x in await db.LevelRewards.Where(x => x.GuildId == user.GuildId).ToListAsync())
            {
                if (userdata.Level >= x.Level && x.Stackable) roles.Add(user.Guild.GetRole(x.Role));

                if (userdata.Level >= x.Level) role = x.Role;
            }

            if (role != 0) roles.Add(user.Guild.GetRole(role));
            return roles;
        }

        private static async Task<List<IRole>> GetRoleCollectionAsync(IGuildUser user, DbService db, Account userdata)
        {
            var roles = new List<IRole>();

            foreach (var x in await db.LevelRewards.Where(x => x.GuildId == user.GuildId).ToListAsync())
                if (userdata.Level >= x.Level)
                    roles.Add(user.Guild.GetRole(x.Role));
            return roles;
        }

        // Cooldown area
        private bool CheckServerCooldown(IGuildUser usr)
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

        private bool CheckGlobalCooldown(IGuildUser usr)
        {
            var check = GlobalExpCooldown.TryGetValue(usr.Id, out var cd);
            if (!check)
            {
                GlobalExpCooldown.TryAdd(usr.Id, DateTime.UtcNow);
                return true;
            }

            if (!((DateTime.UtcNow - cd).TotalSeconds >= 60)) return false;
            GlobalExpCooldown.AddOrUpdate(usr.Id, DateTime.UtcNow, (key, old) => old = DateTime.UtcNow);
            return true;
        }
    }
}