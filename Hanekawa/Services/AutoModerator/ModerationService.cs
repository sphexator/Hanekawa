using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Hanekawa.Events;
using Hanekawa.Extensions;
using Config = Hanekawa.Data.Config;
using System.Text.RegularExpressions;

using Hanekawa.Addons.Database.Tables.Account;

namespace Hanekawa.Services.AutoModerator
{
    public class ModerationService
    {
        private readonly DiscordSocketClient _discord;
        private readonly Config _config;
        private ConcurrentDictionary<ulong, List<ulong>> UrlFilterChannels { get; set; } = new ConcurrentDictionary<ulong, List<ulong>>();
        private ConcurrentDictionary<ulong, List<ulong>> SpamIgnoreChannels { get; set; } = new ConcurrentDictionary<ulong, List<ulong>>();
        private readonly Regex Emote = new Regex(@"/^<a?:(\w+):(\d+)>$/", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex Channel = new Regex(@"/^<#(\d+)>$/", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex Mention = new Regex(@"/^<@!?(\d+)>$/", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex Role = new Regex(@"/^<@&(\d+)>$/", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public enum AutoModActionType
        {
            Invite,
            Spam,
            ScamLink,
            Url,
            Length,
            Toxicity,
            Emote,
            Mention
        }

        public event AsyncEvent<SocketGuildUser> AutoModPermMute;
        public event AsyncEvent<SocketGuildUser, TimeSpan> AutoModTimedMute;
        public event AsyncEvent<SocketGuildUser, AutoModActionType, string> AutoModPermLog;
        public event AsyncEvent<SocketGuildUser, AutoModActionType, TimeSpan, string> AutoModTimedLog;
        public event AsyncEvent<SocketGuildUser, AutoModActionType, int> AutoModFilter;

        public ModerationService(DiscordSocketClient discord, IServiceProvider provider, Config config)
        {
            _discord = discord;
            _config = config;

            _discord.MessageReceived += AutoModInitializer;
            //_discord.UserJoined += GlobalBanChecker;

            using(var db = new DbService())
            {
                foreach (var x in db.UrlFilters)
                {
                    var guild = UrlFilterChannels.GetOrAdd(x.GuildId, new List<ulong>());
                    guild.Add(x.ChannelId);
                }

                foreach (var x in db.SpamIgnores)
                {
                    var guild = SpamIgnoreChannels.GetOrAdd(x.GuildId, new List<ulong>());
                    guild.Add(x.ChannelId);
                }
            }
        }

        public async Task AutoModMute(SocketGuildUser user, AutoModActionType type, TimeSpan time, string reason)
        {
            await AutoModTimedMute(user, time);
            await AutoModTimedLog(user, type, time, reason);
        }

        public async Task AutoModMute(SocketGuildUser user, AutoModActionType type, string reason)
        {
            await AutoModPermMute(user);
            await AutoModPermLog(user, type, reason);
        }

        public async Task<EmbedBuilder> SpamIgnoreHandler(ITextChannel channel)
        {
            using(var db = new DbService())
            {
                var check = await db.SpamIgnores.FindAsync(channel.GuildId, channel.Id);
                if (check == null) return await AddSpamIgnoreChannel(channel, db);
                else return await RemoveSpamIgnoreChannel(channel, db);
            }
        }

        public async Task<EmbedBuilder> UrlFilterHandler(ITextChannel channel)
        {
            using(var db = new DbService())
            {
                var check = await db.UrlFilters.FindAsync(channel.GuildId, channel.Id);
                if (check == null) return await AddUrlFilterChannel(channel, db);
                else return await RemoveUrlFilter(channel, db);
            }
        }

        private async Task<EmbedBuilder> AddSpamIgnoreChannel(ITextChannel channel, DbService db)
        {
            if (!SpamIgnoreChannels.TryGetValue(channel.GuildId, out var guild))return null;
            if (guild.Contains(channel.Id))return null;
            var entry = await db.SpamIgnores.FindAsync(channel.GuildId, channel.Id);
            if (entry != null)
                return null;
            var data = new SpamIgnore
            {
                GuildId = channel.GuildId,
                ChannelId = channel.Id
            };
            await db.SpamIgnores.AddAsync(data);
            await db.SaveChangesAsync();
            guild.Add(channel.Id);
            return new EmbedBuilder().Reply($"Added {channel.Mention} to url spam ignore list.", Color.Green.RawValue);
        }

        private async Task<EmbedBuilder> AddUrlFilterChannel(ITextChannel channel, DbService db)
        {
            if (!UrlFilterChannels.TryGetValue(channel.GuildId, out var guild))return null;
            if (guild.Contains(channel.Id))return null;
            var entry = await db.UrlFilters.FindAsync(channel.GuildId, channel.Id);
            if (entry != null)return null;
            var data = new UrlFilter
            {
                GuildId = channel.GuildId,
                ChannelId = channel.Id
            };
            await db.UrlFilters.AddAsync(data);
            await db.SaveChangesAsync();
            guild.Add(channel.Id);
            return new EmbedBuilder().Reply($"Added {channel.Mention} to url filter.", Color.Green.RawValue);
        }

        private async Task<EmbedBuilder> RemoveSpamIgnoreChannel(ITextChannel channel, DbService db)
        {

            if (!SpamIgnoreChannels.TryGetValue(channel.GuildId, out var guild))return null;
            if (!guild.Contains(channel.Id))return null;
            var entry = await db.SpamIgnores.FindAsync(channel.GuildId, channel.Id);
            if (entry == null)return null;
            db.SpamIgnores.Remove(entry);
            await db.SaveChangesAsync();
            guild.Remove(channel.Id);
            return new EmbedBuilder().Reply($"Removed {channel.Mention} from spam ignore list.", Color.Green.RawValue);
        }

        private async Task<EmbedBuilder> RemoveUrlFilter(ITextChannel channel, DbService db)
        {
            if (!UrlFilterChannels.TryGetValue(channel.GuildId, out var guild)) return null;
            if (!guild.Contains(channel.Id)) return null;
            var entry = await db.UrlFilters.FindAsync(channel.GuildId, channel.Id);
            if (entry == null)return null;
            db.UrlFilters.Remove(entry);
            await db.SaveChangesAsync();
            guild.Remove(channel.Id);
            return new EmbedBuilder().Reply($"Removed {channel.Mention} from url filter.", Color.Green.RawValue);
        }

        private Task GlobalBanChecker(SocketGuildUser user)
        {
            var _ = Task.Run(async() =>
            {
                using(var db = new DbService())
                using(var client = new HttpClient())
                {
                var values = new Dictionary<string, string>
                { { "token", Config.BanApi },
                { "userid", $"{user.Id}" },
                { "version", "3" }
                        };

                    var content = new FormUrlEncodedContent(values);
                    var post = await client.PostAsync("https://bans.discordlist.net/api", content);
                    post.EnsureSuccessStatusCode();
                    var response = await post.Content.ReadAsStringAsync();
                    if (response.ToLower() == "false")return;
                }
            });
            return Task.CompletedTask;
        }

        private Task AutoModInitializer(SocketMessage message)
        {
            var _ = Task.Run(async() =>
            {
                if (!(message is SocketUserMessage msg))return;
                if (msg.Source != MessageSource.User)return;
                if (msg.Author.IsBot)return;
                if (!(msg.Channel is ITextChannel channel))return;
                if (!(msg.Author is SocketGuildUser user))return;

                GuildConfig cfg;
                Account userdata;
                using(var db = new DbService())
                {
                    cfg = await db.GetOrCreateGuildConfig(user.Guild);
                    userdata = await db.GetOrCreateUserData(user);
                }

                var invite = InviteFilter(msg, user, cfg);
                var scam = ScamLinkFilter(msg, user, cfg);
                // var spam = SpamFilter(msg, user, cfg);
                var url = UrlFilter(msg, user, cfg, userdata);
                var world = WordFilter(msg, user, cfg, userdata);
                var length = LengthFilter(msg, user, cfg, userdata);
                var emote = EmoteFilter(msg, user, cfg, userdata);
                var mention = MentionFilter(msg, user, cfg);

                await Task.WhenAll(invite, scam, spam, url, world, length);
            });
            return Task.CompletedTask;
        }

        private async Task InviteFilter(SocketUserMessage msg, IGuildUser user, GuildConfig cfg)
        {
            if (!cfg.FilterInvites)return;
            if (user.GuildPermissions.ManageGuild)return;
            if (msg.Content.IsDiscordInvite())
            {
                try { await msg.DeleteAsync(); }
                catch { /* ignored */ }

                //var invites = await (msg.Channel as SocketTextChannel)?.Guild.GetInvitesAsync();

                await AutoModPermLog(msg.Author as SocketGuildUser, AutoModActionType.Invite, msg.Content);
                await AutoModPermMute(msg.Author as SocketGuildUser);
            }
        }

        private async Task ScamLinkFilter(SocketUserMessage msg, IGuildUser user, GuildConfig cfg)
        {
            if (user.GuildId != 339370914724446208)return;
            if (msg.Content.IsGoogleLink())try { await msg.DeleteAsync(); }
            catch { /* ignored */ }
            if (msg.Content.IsIpGrab())try { await msg.DeleteAsync(); }
            catch { /* ignored */ }
            if (msg.Content.IsScamLink())
            {
                try { await msg.DeleteAsync(); }
                catch { /* ignored */ }
                await AutoModPermMute(msg.Author as SocketGuildUser);
                await AutoModPermLog(msg.Author as SocketGuildUser, AutoModActionType.ScamLink, msg.Content);
            }
        }

        private async Task SpamFilter(SocketUserMessage msg, IGuildUser user, GuildConfig cfg)
        {

        }

        private async Task UrlFilter(SocketUserMessage msg, IGuildUser user, GuildConfig cfg, Account userdata)
        {
            using(var db = new DbService())
            {
                var channel = await db.UrlFilters.FindAsync(user.GuildId, msg.Channel.Id);
                if (channel == null)return;
                if (msg.Channel.Id != channel.ChannelId)return;
                if (msg.Content.IsUrl())try { await msg.DeleteAsync(); }
                catch { /* ignored */ }
                var _ = AutoModFilter(user as SocketGuildUser, AutoModActionType.Url, 0);
            }
        }

        private async Task WordFilter(SocketUserMessage msg, IGuildUser user, GuildConfig cfg, Account userdata)
        {

        }

        private async Task LengthFilter(SocketUserMessage msg, IGuildUser user, GuildConfig cfg, Account userdata)
        {
            if (user.GuildId != 339370914724446208)return;
            if (user.GuildPermissions.ManageMessages)return;
            if (msg.Content.Length >= 1500)
            {
                try { await msg.DeleteAsync(); }
                catch { /* ignored */ }
                await AutoModTimedMute(msg.Author as SocketGuildUser, TimeSpan.FromMinutes(60));
                await AutoModPermLog(msg.Author as SocketGuildUser, AutoModActionType.Length, msg.Content);
            }
        }

        private async Task MentionFilter(SocketUserMessage msg, IGuildUser user, GuildConfig cfg)
        {
            if (!cfg.MentionCountFilter.HasValue || cfg.MentionCountFilter.Value == 0)return;
            if (user.GuildPermissions.ManageMessages)return;
            if (!Mention.IsMatch(msg.Content))return;
            var amount = Mention.Matches(msg.Content).Count;
            if (amount >= cfg.MentionCountFilter)return;
            try { await msg.DeleteAsync(); }
            catch { /* IGNORED */ }
            var _ = AutoModFilter(user as SocketGuildUser, AutoModActionType.Mention, amount);
        }

        private async Task EmoteFilter(SocketUserMessage msg, IGuildUser user, GuildConfig cfg, Account userdata)
        {
            if (!cfg.EmoteCountFilter.HasValue || cfg.EmoteCountFilter.Value == 0)return;
            if (user.GuildPermissions.ManageMessages)return;
            if (!Emote.IsMatch(msg.Content))return;
            var amount = Emote.Matches(msg.Content).Count;
            if (amount < cfg.EmoteCountFilter)return;
            try { await msg.DeleteAsync(); }
            catch { /* IGNORED */ }
            var _ = AutoModFilter(user as SocketGuildUser, AutoModActionType.Emote, amount);
        }
    }
}
