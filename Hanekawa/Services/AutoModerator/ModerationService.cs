﻿using Discord;
using Discord.WebSocket;
using Hanekawa.Events;
using Hanekawa.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Config = Hanekawa.Data.Config;
using System.Text.RegularExpressions;
using Hanekawa.Addons.Database.Tables.Account;

namespace Hanekawa.Services.AutoModerator
{
    public class ModerationService
    {
        private readonly DiscordSocketClient _discord;
        private readonly Config _config;
        private ConcurrentDictionary<ulong, List<ulong>> UrlFilterChannels { get; set; }
            = new ConcurrentDictionary<ulong, List<ulong>>();
        private ConcurrentDictionary<ulong, List<ulong>> SpamFilterChannels { get; set; }
            = new ConcurrentDictionary<ulong, List<ulong>>();
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
            Toxicity
        }

        public event AsyncEvent<SocketGuildUser> AutoModPermMute;
        public event AsyncEvent<SocketGuildUser, TimeSpan> AutoModTimedMute;
        public event AsyncEvent<SocketGuildUser, AutoModActionType, string> AutoModPermLog;
        public event AsyncEvent<SocketGuildUser, AutoModActionType, TimeSpan, string> AutoModTimedLog;

        public ModerationService(DiscordSocketClient discord, IServiceProvider provider, Config config)
        {
            _discord = discord;
            _config = config;

            _discord.MessageReceived += AutoModInitializer;
            _discord.UserJoined += GlobalBanChecker;
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

        private Task GlobalBanChecker(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                using (var client = new HttpClient())
                {
                    var values = new Dictionary<string, string>
                    {
                        {"token", Config.BanApi},
                        {"userid", $"{user.Id}"},
                        {"version", "3"}
                    };

                    var content = new FormUrlEncodedContent(values);
                    var post = await client.PostAsync("https://bans.discordlist.net/api", content);
                    post.EnsureSuccessStatusCode();
                    var response = await post.Content.ReadAsStringAsync();
                    if (response.ToLower() == "false") return;
                }
            });
            return Task.CompletedTask;
        }

        private Task AutoModInitializer(SocketMessage message)
        {
            var _ = Task.Run(async () =>
            {
                if (!(message is SocketUserMessage msg)) return;
                if (msg.Source != MessageSource.User) return;
                if (msg.Author.IsBot) return;
                if (!(msg.Channel is ITextChannel channel)) return;
                if (!(msg.Author is SocketGuildUser user)) return;

                GuildConfig cfg;
                Account userdata;
                using (var db = new DbService())
                {
                    cfg = await db.GetOrCreateGuildConfig(user.Guild);
                    userdata = await db.GetOrCreateUserData(user);
                }

                var invite = InviteFilter(msg, user, cfg);
                var scam = ScamLinkFilter(msg, user, cfg);
                var spam = SpamFilter(msg, user, cfg);
                var url = UrlFilter(msg, user, cfg);
                var world = WordFilter(msg, user, cfg);
                var length = LengthFilter(msg, user, cfg);

                await Task.WhenAll(invite, scam, spam, url, world, length);
            });
            return Task.CompletedTask;
        }

        private async Task InviteFilter(SocketUserMessage msg, IGuildUser user, GuildConfig cfg)
        {
            if (!cfg.FilterInvites) return;
            if (user.GuildPermissions.ManageGuild) return;
            if (msg.Content.IsDiscordInvite())
            {
                try { await msg.DeleteAsync(); } catch { /* ignored */ }

                //var invites = await (msg.Channel as SocketTextChannel)?.Guild.GetInvitesAsync();
                
                await AutoModPermLog(msg.Author as SocketGuildUser, AutoModActionType.Invite, msg.Content);
                await AutoModPermMute(msg.Author as SocketGuildUser);
            }
        }

        private async Task ScamLinkFilter(SocketUserMessage msg, IGuildUser user, GuildConfig cfg)
        {
            if (user.GuildId != 339370914724446208) return;
            if (msg.Content.IsGoogleLink()) try { await msg.DeleteAsync(); } catch { /* ignored */ }
            if (msg.Content.IsIpGrab()) try { await msg.DeleteAsync(); } catch { /* ignored */ }
            if (msg.Content.IsScamLink())
            {
                try { await msg.DeleteAsync(); } catch { /* ignored */ }
                await AutoModPermMute(msg.Author as SocketGuildUser);
                await AutoModPermLog(msg.Author as SocketGuildUser, AutoModActionType.ScamLink, msg.Content);
            }
        }

        private async Task SpamFilter(SocketUserMessage msg, IGuildUser user, GuildConfig cfg)
        {

        }

        private async Task UrlFilter(SocketUserMessage msg, IGuildUser user, GuildConfig cfg, Account userdata)
        {
            //if (msg.Content.IsUrl()) try { await msg.DeleteAsync(); } catch { /* ignored */ }
        }

        private async Task WordFilter(SocketUserMessage msg, IGuildUser user, GuildConfig cfg, Account userdata)
        {

        }

        private async Task LengthFilter(SocketUserMessage msg, IGuildUser user, GuildConfig cfg, Account userdata)
        {
            if (user.GuildId != 339370914724446208) return;
            if (user.GuildPermissions.ManageMessages) return;
            if (msg.Content.Length >= 1500)
            {
                try { await msg.DeleteAsync(); } catch { /* ignored */ }
                await AutoModTimedMute(msg.Author as SocketGuildUser, TimeSpan.FromMinutes(60));
                await AutoModPermLog(msg.Author as SocketGuildUser, AutoModActionType.Length, msg.Content);
            }
        }

        private async Task MentionFilter(SocketUserMessage msg, IGuildUser user, GuildConfig cfg)
        {
            if (cfg.MentionCountFilter == null || cfg.MentionCountFilter == 0) return;
            if (user.GuildPermissions.ManageMessages) return;
            if (!Mention.IsMatch(msg.Content)) return;
            var amount = Mention.Matches(msg.Content).Count;
            if (amount >= cfg.MentionCountFilter)
            {
                try { await msg.DeleteAsync(); } catch{ }
            }
        }

        private async Task EmoteFilter(SocketUserMessage msg, IGuildUser user, GuildConfig cfg, Account userdata)
        {
            if (cfg.EmoteCountFilter == null || cfg.EmoteCountFilter == 0) return;
            if (user.GuildPermissions.ManageMessages) return;
            if (!Emote.IsMatch(msg.Content)) return;
            var amount = Emote.Matches(msg.Content).Count;
            if (amount >= cfg.EmoteCountFilter)
            {
                try {await msg.DeleteAsync(); } catch{ }
            }
        }
    }
}