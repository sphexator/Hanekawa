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

namespace Jibril.Services.AutoModerator
{
    public class ModerationService
    {
        private readonly DiscordSocketClient _discord;
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

        private static Task GlobalBanChecker(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                using (var client = new HttpClient())
                {
                    var values = new Dictionary<string, string>
                    {
                        {"token", $"E7puJQIwyp"},
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
            if (msg.Content.IsUrl()) try { await msg.DeleteAsync(); } catch { /* ignored */ }
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

    /*
                     try
                {
                    if (!(rawMessage.Author is SocketGuildUser user)) return;
                    var staffCheck = user.GuildPermissions.ManageMessages;
                    if (staffCheck != true)
                    {
                        if (rawMessage.Content.IsDiscordInvite())
                            try
                            {
                                await rawMessage.DeleteAsync();

                                var guild = _discord.Guilds.First(x => x.Id == 339370914724446208);
                                var ch = guild.TextChannels.First(x => x.Id == 339381104534355970);
                                var role = guild.Roles.FirstOrDefault(r => r.Name == "Mute");
                                await user.AddRoleAsync(role);
                                await user.ModifyAsync(x => x.Mute = true);

                                const string reason = "Discord invite link";
                                var msg = $"{rawMessage.Content}";
                                var embed = AutoModResponse(user, reason, msg);

                                await ch.SendMessageAsync("", false, embed.Build());
                                return;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                return;
                            }

                        if (rawMessage.Content.IsGoogleLink()) await rawMessage.DeleteAsync();
                        if (rawMessage.Content.IsIpGrab()) await rawMessage.DeleteAsync();
                        if (rawMessage.Content.IsScamLink())
                            try
                            {
                                await rawMessage.DeleteAsync();

                                var guild = _discord.Guilds.First(x => x.Id == 339370914724446208);
                                var ch = guild.TextChannels.First(x => x.Id == 339381104534355970);

                                var role = guild.Roles.FirstOrDefault(r => r.Name == "Mute");
                                await user.AddRoleAsync(role);
                                await user.ModifyAsync(x => x.Mute = true);

                                const string reason = "Scam/malicious link";
                                var msg = $"{rawMessage.Content}";
                                var embed = AutoModResponse(user, reason, msg);

                                await ch.SendMessageAsync("", false, embed.Build());
                                return;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                return;
                            }

                        if (rawMessage.Content.IsPornLink() && ((ITextChannel)rawMessage.Channel).IsNsfw != true)
                            try
                            {
                                await rawMessage.DeleteAsync();

                                var guild = _discord.Guilds.First(x => x.Id == 339370914724446208);
                                var ch = guild.TextChannels.First(x => x.Id == 339381104534355970);

                                var role = guild.Roles.FirstOrDefault(r => r.Name == "Mute");
                                await user.AddRoleAsync(role);
                                await user.ModifyAsync(x => x.Mute = true);

                                const string reason = "Porn link";
                                var msg = $"{rawMessage.Content}";
                                var embed = AutoModResponse(user, reason, msg);

                                await ch.SendMessageAsync("", false, embed.Build());
                                return;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                return;
                            }

                        if (rawMessage.Content.Length >= 1500)
                            try
                            {
                                await rawMessage.DeleteAsync();
                                var guild = _discord.Guilds.First(x => x.Id == 339370914724446208);
                                var ch = guild.TextChannels.First(x => x.Id == 339381104534355970);
                                var role = guild.Roles.FirstOrDefault(r => r.Name == "Mute");
                                await user.AddRoleAsync(role);
                                await user.ModifyAsync(x => x.Mute = true);
                                const string reason = "Character count >= 1500";
                                const string msg = "Too Long Didn't Read.";
                                var embed = AutoModResponse(user, reason, msg);
                                embed.ThumbnailUrl = "http://i0.kym-cdn.com/photos/images/original/000/834/934/f64.gif";

                                await ch.SendMessageAsync("", false, embed.Build());
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        /*
                        if (rawMessage.Content.IsUrl())
                        {
                            var userdata = DatabaseService.UserData(rawMessage.Author).FirstOrDefault();
                            if (userdata.Level >= 10) return;
                            await rawMessage.DeleteAsync();
                            var ch = await _discord.GetUser(111123736660324352).GetOrCreateDMChannelAsync();
                            await ch.SendMessageAsync(
                                $"{rawMessage.Author.Username}#{rawMessage.Author.DiscriminatorValue} ({rawMessage.Author.Id}) - Posted in {rawMessage.Channel.Name}\n" +
                                $"{rawMessage.Content}");
                        }
                        
}
                }
                catch
                {
                    // ignored
                }
     */
}