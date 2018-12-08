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
using Hanekawa.Entities.Interfaces;
using Hanekawa.Events;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Services.Level.Util;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Services.Level
{
    public class LevelingService : IHanaService, IRequiredService
    {
        private readonly Calculate _calc;
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;

        private readonly ConcurrentDictionary<ulong, List<ulong>> _serverCategoryReduction =
            new ConcurrentDictionary<ulong, List<ulong>>();

        private readonly ConcurrentDictionary<ulong, List<ulong>> _serverChannelReduction =
            new ConcurrentDictionary<ulong, List<ulong>>();

        public LevelingService(DiscordSocketClient discord, Calculate calc, DbService db)
        {
            _client = discord;
            _calc = calc;
            _db = db;

            _client.MessageReceived += ServerMessageExpAsync;
            _client.MessageReceived += GlobalMessageExpAsync;
            _client.UserVoiceStateUpdated += VoiceExpAsync;
            _client.UserJoined += GiveRolesBackAsync;

            _client.UserJoined += UserActiveAsync;
            _client.UserLeft += UserDeactivateAsync;

            foreach (var x in _db.GuildConfigs)
            {
                var expEvent = db.LevelExpEvents.Find(x.GuildId);
                if (expEvent == null)
                {
                    ExpMultiplier.TryAdd(x.GuildId, x.ExpMultiplier);
                }
                else if (expEvent.Time - TimeSpan.FromMinutes(2) <= DateTime.UtcNow)
                {
                    ExpMultiplier.TryAdd(x.GuildId, x.ExpMultiplier);
                    RemoveFromDatabase(x.GuildId);
                }
                else
                {
                    var after = expEvent.Time - DateTime.UtcNow;
                    ExpEventHandler(expEvent.GuildId, expEvent.Multiplier, x.ExpMultiplier, expEvent.MessageId,
                        expEvent.ChannelId, after);
                }
            }

            foreach (var x in _db.LevelExpReductions)
            {
                if (x.Channel)
                {
                    var channels = _serverChannelReduction.GetOrAdd(x.GuildId, new List<ulong>());
                    channels.Add(x.ChannelId);
                }

                if (!x.Category) continue;
                {
                    var channels = _serverCategoryReduction.GetOrAdd(x.GuildId, new List<ulong>());
                    channels.Add(x.ChannelId);
                }
            }

            Console.WriteLine("Leveling service loaded");
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
            IUserMessage message = null;
            var cfg = await _db.GetOrCreateGuildConfig(guild as SocketGuild);
            if (announce) message = await AnnounceExpEventAsync(cfg, guild, multiplier, after, fallbackChannel);
            ExpEventHandler(guild.Id, multiplier, cfg.ExpMultiplier, message?.Id, message?.Channel.Id, after);
            await EventAddOrUpdateDatabaseAsync(guild.Id, multiplier, message?.Id, message?.Channel.Id, after);
        }

        public async Task<EmbedBuilder> ReducedExpManager(ICategoryChannel category = null, ITextChannel channel = null)
        {
            if (category != null)
            {
                var channels = _serverCategoryReduction.GetOrAdd(category.GuildId, new List<ulong>());
                if (channels.Contains(category.Id)) return await RemoveReducedExp(category);
                return await AddReducedExp(category);
            }

            if (channel == null) return null;
            {
                var channels = _serverChannelReduction.GetOrAdd(channel.GuildId, new List<ulong>());
                if (channels.Contains(channel.Id)) return await RemoveReducedExp(null, channel);
                return await AddReducedExp(null, channel);
            }
        }

        private async Task<EmbedBuilder> AddReducedExp(IGuildChannel category = null, IGuildChannel channel = null)
        {
            if (category != null)
            {
                var channels = _serverCategoryReduction.GetOrAdd(category.GuildId, new List<ulong>());
                channels.Add(category.Id);
                var data = new LevelExpReduction
                {
                    GuildId = category.GuildId,
                    ChannelId = category.Id,
                    Channel = false,
                    Category = true
                };
                await _db.LevelExpReductions.AddAsync(data);
                await _db.SaveChangesAsync();
                return new EmbedBuilder().CreateDefault($"Added {category.Name} to reduced exp list", Color.Green.RawValue);
            }

            if (channel == null) return null;
            {
                var channels = _serverChannelReduction.GetOrAdd(channel.GuildId, new List<ulong>());
                channels.Add(channel.Id);
                var data = new LevelExpReduction
                {
                    GuildId = channel.GuildId,
                    ChannelId = channel.Id,
                    Channel = true,
                    Category = false
                };
                await _db.LevelExpReductions.AddAsync(data);
                await _db.SaveChangesAsync();
                return new EmbedBuilder().CreateDefault($"Added {channel.Name} to reduced exp list", Color.Green.RawValue);
            }
        }

        private async Task<EmbedBuilder> RemoveReducedExp(IGuildChannel category = null, IGuildChannel channel = null)
        {
            if (category != null)
            {
                var channels = _serverCategoryReduction.GetOrAdd(category.GuildId, new List<ulong>());
                channels.Remove(category.Id);
                var data = await _db.LevelExpReductions.FindAsync(category.GuildId, category.Id);
                _db.LevelExpReductions.Remove(data);
                await _db.SaveChangesAsync();
                return new EmbedBuilder().CreateDefault($"Removed {category.Name} from reduced exp list", Color.Green.RawValue);
            }

            if (channel == null) return null;
            {
                var channels = _serverChannelReduction.GetOrAdd(channel.GuildId, new List<ulong>());
                channels.Remove(channel.Id);
                var data = await _db.LevelExpReductions.FindAsync(channel.GuildId, channel.Id);
                _db.LevelExpReductions.Remove(data);
                await _db.SaveChangesAsync();
                return new EmbedBuilder().CreateDefault($"Removed {channel.Name} from reduced exp list", Color.Green.RawValue);
            }
        }

        private void ExpEventHandler(ulong guildId, uint multiplier, uint defaultMulti, ulong? messageId,
            ulong? channelId, TimeSpan after)
        {
            ExpMultiplier.AddOrUpdate(guildId, multiplier, (key, old) => multiplier);
            var toAdd = new Timer(async _ =>
            {
                try
                {
                    ExpMultiplier.AddOrUpdate(guildId, defaultMulti, (key, old) => defaultMulti);
                    if (messageId.HasValue && channelId.HasValue)
                        if (await _client.GetGuild(guildId).GetTextChannel(channelId.Value)
                            .GetMessageAsync(messageId.Value) is IUserMessage msg)
                        {
                            var upd = msg.Embeds.First().ToEmbedBuilder();
                            upd.Color = Color.Red;
                            await msg.ModifyAsync(x => x.Embed = upd.Build());
                        }

                    RemoveFromDatabase(guildId);
                    ExpEvent.Remove(guildId, out var _);
                }
                catch
                {
                    ExpMultiplier.AddOrUpdate(guildId, defaultMulti, (key, old) => defaultMulti);
                    RemoveFromDatabase(guildId);
                }
            }, null, after, Timeout.InfiniteTimeSpan);
            ExpEvent.AddOrUpdate(guildId, key => toAdd, (key, old) =>
            {
                old.Change(Timeout.Infinite, Timeout.Infinite);
                return toAdd;
            });
        }

        private async Task EventAddOrUpdateDatabaseAsync(ulong guildId, uint multiplier,
            ulong? message, ulong? channel, TimeSpan after)
        {
            var check = await _db.LevelExpEvents.FindAsync(guildId);
            if (check == null)
            {
                var data = new LevelExpEvent
                {
                    GuildId = guildId,
                    MessageId = message,
                    ChannelId = channel,
                    Multiplier = multiplier,
                    Time = DateTime.UtcNow + after
                };
                await _db.LevelExpEvents.AddAsync(data);
                await _db.SaveChangesAsync();
            }
            else
            {
                check.ChannelId = channel;
                check.Time = DateTime.UtcNow + after;
                check.Multiplier = multiplier;
                check.MessageId = message;
                await _db.SaveChangesAsync();
            }
        }

        private void RemoveFromDatabase(ulong guildId)
        {
            var check = _db.LevelExpEvents.FirstOrDefault(x => x.GuildId == guildId);
            if (check == null) return;
            _db.LevelExpEvents.Remove(check);
            _db.SaveChanges();
        }

        private async Task<IUserMessage> AnnounceExpEventAsync(GuildConfig cfg, IGuild guild,
            uint multiplier, TimeSpan after, IMessageChannel fallbackChannel)
        {
            var check = await _db.LevelExpEvents.FindAsync(guild.Id);
            if (check?.ChannelId == null || !check.MessageId.HasValue)
                return await PostAnnouncementAsync(guild, cfg, multiplier, after, fallbackChannel);
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
                var embed = new EmbedBuilder().CreateDefault($"A {multiplier}x exp event has started!\n" +
                                                             $"Duration: {after.Humanize()} ( {after} )");
                embed.Title = "Exp Event";
                embed.Timestamp = DateTimeOffset.UtcNow + after;
                embed.Footer = new EmbedFooterBuilder { Text = "Ends:"};
                var msg = await channel.SendMessageAsync(null, false, embed.Build());
                return msg;
            }

            await fallbackChannel.SendMessageAsync(null, false, 
                new EmbedBuilder().CreateDefault("No event channel has been setup.",
                Color.Red.RawValue).Build());
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

                _serverChannelReduction.TryGetValue(channel.GuildId, out var channelList);
                _serverCategoryReduction.TryGetValue(channel.GuildId, out var category);
                var reduced = channelList != null && channelList.Contains(channel.Id) || category != null &&
                              channel.CategoryId.HasValue && category.Contains(channel.CategoryId.Value);

                ExpMultiplier.TryGetValue(((IGuildChannel) msg.Channel).GuildId, out var multi);
                var userdata = await _db.GetOrCreateUserData(user);
                var cfg = await _db.GetOrCreateGuildConfig(((IGuildChannel) msg.Channel).Guild);
                var exp = _calc.GetMessageExp(msg, reduced) * cfg.ExpMultiplier * multi;
                var nxtLvl = _calc.GetServerLevelRequirement(userdata.Level);

                userdata.LastMessage = DateTime.UtcNow;
                if (!userdata.FirstMessage.HasValue) userdata.FirstMessage = DateTime.UtcNow;

                userdata.TotalExp = userdata.TotalExp + exp;
                userdata.Credit = userdata.Credit + _calc.GetMessageCredit();

                if (userdata.Exp + exp >= nxtLvl)
                {
                    userdata.Level = userdata.Level + 1;
                    userdata.Exp = userdata.Exp + exp - nxtLvl;
                    await NewLevelManagerAsync(userdata, user);
                    var __ = ServerLevel?.Invoke(user, userdata);
                }
                else
                {
                    userdata.Exp = userdata.Exp + exp;
                }

                await _db.SaveChangesAsync();
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
                if (!(msg.Channel is ITextChannel channel)) return;
                if (!(msg.Author is SocketGuildUser user)) return;

                if (!CheckGlobalCooldown(user)) return;

                _serverChannelReduction.TryGetValue(user.Guild.Id, out var channelList);
                _serverCategoryReduction.TryGetValue(user.Guild.Id, out var category);
                var reduced = channelList != null && channelList.Contains(channel.Id) || category != null &&
                              channel.CategoryId.HasValue && category.Contains(channel.CategoryId.Value);

                var userdata = await _db.GetOrCreateGlobalUserData(user);
                var exp = _calc.GetMessageExp(msg, reduced);
                var nextLevel = _calc.GetGlobalLevelRequirement(userdata.Level);
                userdata.TotalExp = userdata.TotalExp + exp;
                userdata.Credit = userdata.Credit + _calc.GetMessageCredit();
                if (userdata.Exp + exp >= nextLevel)
                {
                    userdata.Exp = userdata.Exp + exp - nextLevel;
                    userdata.Level = userdata.Level + 1;
                    var __ = GlobalLevel?.Invoke(user, userdata);
                }
                else
                {
                    userdata.Exp = userdata.Exp + exp;
                }

                await _db.SaveChangesAsync();
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
                    var userdata = await _db.GetOrCreateUserData(gusr);
                    var cfg = await _db.GetOrCreateGuildConfig(gusr.Guild);
                    var oldVc = oldState.VoiceChannel;
                    var newVc = newState.VoiceChannel;
                    if (newVc != null && oldVc == null)
                    {
                        userdata.VoiceExpTime = DateTime.UtcNow;
                        await _db.SaveChangesAsync();
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
                        await NewLevelManagerAsync(userdata, gusr);
                    }
                    else
                    {
                        userdata.Exp = userdata.Exp + exp;
                    }

                    await _db.SaveChangesAsync();
                    await InVoice?.Invoke(gusr, DateTime.UtcNow - userdata.VoiceExpTime);
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

        private static Task UserDeactivateAsync(SocketGuildUser user)
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
        private async Task NewLevelManagerAsync(Account userdata, IGuildUser user)
        {
            var roles = await _db.LevelRewards.Where(x => x.GuildId == user.GuildId).ToListAsync();
            var role = GetLevelUpRole(userdata.Level, user, roles);
            var cfg = await _db.GetOrCreateGuildConfig(user.Guild as SocketGuild);

            if (role == null)
            {
                await RoleCheckAsync(user, cfg, userdata);
                return;
            }

            if (!cfg.StackLvlRoles) await RemoveLevelRolesAsync(user, roles);
            await user.AddRoleAsync(role);
        }

        private IRole GetLevelUpRole(uint level, IGuildUser user, IEnumerable<LevelReward> rolesRewards)
        {
            var roleId = rolesRewards.FirstOrDefault(x => x.Level == level);
            return roleId == null ? null : _client.GetGuild(user.GuildId).GetRole(roleId.Role);
        }

        private static async Task RemoveLevelRolesAsync(IGuildUser user, IEnumerable<LevelReward> rolesRewards)
        {
            foreach (var x in rolesRewards)
            {
                if (x.Stackable) continue;
                if (user.RoleIds.Contains(x.Role)) await user.RemoveRoleAsync(user.Guild.GetRole(x.Role));
            }
        }

        private Task GiveRolesBackAsync(SocketGuildUser user)
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
                        var roleCollection = await GetRoleCollectionAsync(user, userdata);
                        await user.AddRolesAsync(roleCollection);
                        return;
                    }

                    var singleRole = await GetRoleSingleAsync(user, userdata);
                    await user.AddRolesAsync(singleRole);
                }
            });
            return Task.CompletedTask;
        }

        private async Task RoleCheckAsync(IGuildUser user, GuildConfig cfg, Account userdata)
        {
            List<IRole> roles;
            if (cfg.StackLvlRoles) roles = await GetRoleCollectionAsync(user, userdata);
            else roles = await GetRoleSingleAsync(user, userdata);
            var missingRoles = new List<IRole>();
            var currentUser = await user.Guild.GetCurrentUserAsync();
            foreach (var x in roles)
                if (!user.RoleIds.Contains(x.Id) && (currentUser as SocketGuildUser).HierarchyCheck(x))
                {
                    missingRoles.Add(x);
                }

            if (missingRoles.Count == 0) return;
            if (missingRoles.Count > 1) await user.AddRolesAsync(missingRoles);
            else await user.AddRoleAsync(missingRoles.FirstOrDefault());
        }

        private async Task<List<IRole>> GetRoleSingleAsync(IGuildUser user, Account userdata)
        {
            var roles = new List<IRole>();
            ulong role = 0;
            var currentUser = await user.Guild.GetCurrentUserAsync();
            foreach (var x in await _db.LevelRewards.Where(x => x.GuildId == user.GuildId).ToListAsync())
            {
                if (userdata.Level >= x.Level && x.Stackable)
                {
                    var getRole = user.Guild.GetRole(x.Role);
                    if ((currentUser as SocketGuildUser).HierarchyCheck(getRole)) roles.Add(getRole);
                }

                if (userdata.Level >= x.Level) role = x.Role;
            }

            if (role == 0) return roles;
            var getSingleRole = user.Guild.GetRole(role);
            if((currentUser as SocketGuildUser).HierarchyCheck(getSingleRole)) roles.Add(getSingleRole);
            return roles;
        }

        private async Task<List<IRole>> GetRoleCollectionAsync(IGuildUser user, Account userdata)
        {
            var roles = new List<IRole>();
            var currentUser = await user.Guild.GetCurrentUserAsync();
            foreach (var x in await _db.LevelRewards.Where(x => x.GuildId == user.GuildId).ToListAsync())
                if (userdata.Level >= x.Level)
                {
                    var getRole = user.Guild.GetRole(x.Role);
                    if((currentUser as SocketGuildUser).HierarchyCheck(getRole)) roles.Add(getRole);
                }
            return roles;
        }

        // Cooldown area
        private bool CheckServerCooldown(IGuildUser usr)
        {
            var cooldown = ServerExpCooldown.GetOrAdd(usr.GuildId, new ConcurrentDictionary<ulong, DateTime>());
            var check = cooldown.TryGetValue(usr.Id, out var cd);
            if (!check)
            {
                cooldown.TryAdd(usr.Id, DateTime.UtcNow);
                return true;
            }

            if (!((DateTime.UtcNow - cd).TotalSeconds >= 60)) return false;
            cooldown.AddOrUpdate(usr.Id, DateTime.UtcNow, (key, old) => DateTime.UtcNow);
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
            GlobalExpCooldown.AddOrUpdate(usr.Id, DateTime.UtcNow, (key, old) => DateTime.UtcNow);
            return true;
        }
    }
}