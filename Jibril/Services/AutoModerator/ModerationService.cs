using Discord;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Jibril.Events;

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
            Url
        }

        public event AsyncEvent<IGuildUser, AutoModActionType> AutoModPermMute;
        public event AsyncEvent<IGuildUser, AutoModActionType, TimeSpan> AutoModTimedMute;
        public event AsyncEvent<IGuildUser, AutoModActionType> AutoModPermLog;
        public event AsyncEvent<IGuildUser, AutoModActionType, TimeSpan> AutoModTimedLog;

        public ModerationService(DiscordSocketClient discord, IServiceProvider provider, IConfiguration config)
        {
            _discord = discord;

            _discord.MessageReceived += AutoModInitializer;
            _discord.UserJoined += GlobalBanChecker;
        }

        private Task GlobalBanChecker(SocketGuildUser user)
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
                    var post = client.PostAsync("https://bans.discordlist.net/api", content).Result;
                    post.EnsureSuccessStatusCode();
                    var response = post.Content.ReadAsStringAsync().Result;
                    if (response.ToLower() == "false") return;
                    var embed = EmbedBuilder(response, user);
                    await _discord.GetGuild(339370914724446208).GetTextChannel(339380827379204097)
                        .SendMessageAsync("", false, embed.Build());
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
                        */
                    }
                }
                catch
                {
                    // ignored
                }
            });
            return Task.CompletedTask;
        }

        private async Task InviteFilter(SocketMessage msg)
        {
            if (msg.Content.IsDiscordInvite())
            {
                await msg.DeleteAsync();
                await AutoModPermLog(msg.Author as SocketGuildUser, AutoModActionType.Invite);
                await AutoModPermMute(msg.Author as SocketGuildUser, AutoModActionType.Invite);
            }
        }

        private async Task SpamFilter(SocketMessage msg)
        {
            if (msg.Content.IsGoogleLink()) await msg.DeleteAsync();
            if (msg.Content.IsIpGrab()) await msg.DeleteAsync();
            if (msg.Content.IsScamLink())
            {
                await msg.DeleteAsync();
                await AutoModPermMute(msg.Author as SocketGuildUser, AutoModActionType.ScamLink);
                await AutoModPermLog(msg.Author as SocketGuildUser, AutoModActionType.ScamLink);
            }
        }

        private async Task ScamLinkFilter(SocketMessage msg)
        {

        }

        private async Task UrlFilter(SocketMessage msg)
        {
            if (msg.Content.IsUrl())
            {
                await msg.DeleteAsync();
                var ch = await _discord.GetUser(111123736660324352).GetOrCreateDMChannelAsync();
                await ch.SendMessageAsync(
                    $"{msg.Author.Username}#{msg.Author.DiscriminatorValue} ({msg.Author.Id}) - Posted in {msg.Channel.Name}\n" +
                    $"{msg.Content}");
            }
        }

        private async Task LengthFilter(SocketMessage msg)
        {
            if (msg.Content.Length >= 1500)
            {
                await msg.DeleteAsync();
                AutoModTimedMute(msg.Author as SocketGuildUser, AutoModActionType.Spam, 30);
            }
        }

        private static string[] FilterString(string x)
        {
            var s = x.Split(",", StringSplitOptions.RemoveEmptyEntries);
            return s;
        }
    }

    public class ToxicityList
    {
        public double ToxicityValue { get; set; }
        public int Toxicitymsgcount { get; set; }
        public double Toxicityavg { get; set; }
    }
}