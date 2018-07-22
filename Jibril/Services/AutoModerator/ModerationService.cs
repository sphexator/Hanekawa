using Discord;
using Discord.WebSocket;
using Jibril.Events;
using Jibril.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Jibril.Data;
using Jibril.Services.Entities;

namespace Jibril.Services.AutoModerator
{
    public class ModerationService
    {
        private readonly DiscordSocketClient _discord;
        private readonly Config _config;
        private ConcurrentDictionary<ulong, List<ulong>> UrlFilterChannels { get; set; }
            = new ConcurrentDictionary<ulong, List<ulong>>();
        private ConcurrentDictionary<ulong, List<ulong>> SpamFilterChannels { get; set; }
            = new ConcurrentDictionary<ulong, List<ulong>>();

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

        public ModerationService(DiscordSocketClient discord, IServiceProvider provider, IConfiguration config)
        {
            _discord = discord;

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
                        {"token", _config.BanApi},
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

        private Task AutoModInitializer(SocketMessage rawMessage)
        {
            var _ = Task.Run(async () =>
            {
                if (!(rawMessage is SocketUserMessage message)) return;
                if (message.Source != MessageSource.User) return;

                var invite = InviteFilter(message);
                var scam = ScamLinkFilter(message);
                var spam = SpamFilter(message);
                var url = UrlFilter(message);
                var world = WordFilter(message);
                var length = LengthFilter(message);

                await Task.WhenAll(invite, scam, spam, url, world, length);
            });
            return Task.CompletedTask;
        }

        private async Task InviteFilter(SocketMessage msg)
        {
            if (msg.Content.IsDiscordInvite())
            {
                try { await msg.DeleteAsync(); } catch { /* ignored */ }

                var invites = await (msg.Channel as SocketTextChannel)?.Guild.GetInvitesAsync();
                
                await AutoModPermLog(msg.Author as SocketGuildUser, AutoModActionType.Invite, msg.Content);
                await AutoModPermMute(msg.Author as SocketGuildUser);
            }
        }

        private async Task ScamLinkFilter(SocketMessage msg)
        {
            if (msg.Content.IsGoogleLink()) try { await msg.DeleteAsync(); } catch { /* ignored */ }
            if (msg.Content.IsIpGrab()) try { await msg.DeleteAsync(); } catch { /* ignored */ }
            if (msg.Content.IsScamLink())
            {
                try { await msg.DeleteAsync(); } catch { /* ignored */ }
                await AutoModPermMute(msg.Author as SocketGuildUser);
                await AutoModPermLog(msg.Author as SocketGuildUser, AutoModActionType.ScamLink, msg.Content);
            }
        }

        private async Task SpamFilter(SocketMessage msg)
        {

        }

        private async Task UrlFilter(SocketMessage msg)
        {
            //if (msg.Content.IsUrl()) try { await msg.DeleteAsync(); } catch { /* ignored */ }
        }

        private async Task WordFilter(SocketMessage msg)
        {

        }

        private async Task LengthFilter(SocketMessage msg)
        {
            if (msg.Content.Length >= 1500)
            {
                try { await msg.DeleteAsync(); } catch { /* ignored */ }
                await AutoModTimedMute(msg.Author as SocketGuildUser, TimeSpan.FromMinutes(60));
                await AutoModPermLog(msg.Author as SocketGuildUser, AutoModActionType.Length, msg.Content);
            }
        }

        private static string[] FilterString(string x)
        {
            var s = x.Split(",", StringSplitOptions.RemoveEmptyEntries);
            return s;
        }
    }
}