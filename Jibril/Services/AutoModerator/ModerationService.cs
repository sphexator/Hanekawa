using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Extensions;
using Jibril.Modules.Administration.Services;
using Jibril.Services.AutoModerator.Perspective.Models;
using Jibril.Services.Common;
using Newtonsoft.Json;
using Quartz.Util;

namespace Jibril.Services.AutoModerator
{
    public class ModerationService
    {
        private readonly DiscordSocketClient _discord;
        private IServiceProvider _provider;

        public ModerationService(DiscordSocketClient discord, IServiceProvider provider)
        {
            _discord = discord;
            _provider = provider;

            _discord.MessageReceived += _discord_MessageReceived;
            //_discord.MessageReceived += PerspectiveApi;
        }

        private Task _discord_MessageReceived(SocketMessage rawMessage)
        {
            var _ = Task.Run(async () =>
            {
                if (!(rawMessage is SocketUserMessage message)) return;
                if (message.Source != MessageSource.User) return;
                try
                {
                    var user = rawMessage.Author as SocketGuildUser;
                    if (user == null) return;
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

                                var reason = "Discord invite link";
                                var msg = $"{rawMessage.Content}";
                                var embed = AutoModResponse(user, reason, msg);

                                await ch.SendMessageAsync("", false, embed.Build());
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        if (rawMessage.Content.IsGoogleLink())
                            await rawMessage.DeleteAsync();
                        if (rawMessage.Content.IsScamLink())
                            try
                            {
                                await rawMessage.DeleteAsync();

                                var guild = _discord.Guilds.First(x => x.Id == 339370914724446208);
                                var ch = guild.TextChannels.First(x => x.Id == 339381104534355970);

                                var role = guild.Roles.FirstOrDefault(r => r.Name == "Mute");
                                await user.AddRoleAsync(role);
                                await user.ModifyAsync(x => x.Mute = true);

                                var reason = "Scam/malicious link";
                                var msg = $"{rawMessage.Content}";
                                var embed = AutoModResponse(user, reason, msg);

                                await ch.SendMessageAsync("", false, embed.Build());
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
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
                                var reason = "Character count >= 1500";
                                var msg = "Too Long Didn't Read.";
                                var embed = AutoModResponse(user, reason, msg);
                                embed.ThumbnailUrl = "http://i0.kym-cdn.com/photos/images/original/000/834/934/f64.gif";

                                await ch.SendMessageAsync("", false, embed.Build());
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                    }
                }
                catch
                {
                }
            });
            return Task.CompletedTask;
        }

        private Task PerspectiveApi(SocketMessage msg)
        {
            var _ = Task.Run(() =>
            {
                if (!(msg is SocketUserMessage message)) return;
                if (message.Source != MessageSource.User) return;
                try
                {
                    var content = msg.Content;
                    var regex = new Regex("((:)([a-z]).*?(:))|((<@)([0-9]).*?(>))|((<@!)([0-9]).*?(>))",
                        RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var filter = regex.Replace(content, "");
                    if (filter.IsNullOrWhiteSpace()) return;
                    var request = new AnalyzeCommentRequest(filter);

                    var response = SendNudes(request);
                    var score = response.AttributeScores.TOXICITY.SummaryScore.Value;
                    Console.WriteLine(
                        $"{DateTime.Now.ToLongTimeString()} | TOXICITY SERVICE | {msg.Author.Id} | Toxicity score:{score} | {msg.Author.Username}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
            return Task.CompletedTask;
        }

        private AnalyzeCommentResponse SendNudes(AnalyzeCommentRequest request)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8,
                    "application/json");
                var response = client
                    .PostAsync($"https://commentanalyzer.googleapis.com/v1alpha1/comments:analyze?key={Token.key}",
                        content).Result;
                response.EnsureSuccessStatusCode();
                var data = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<AnalyzeCommentResponse>(data);
                return result;
            }
        }
         
        private EmbedBuilder AutoModResponse(IUser user, string reason, string message)
        {
            var time = DateTime.Now;
            AdminDb.AddActionCase(user, time);
            var caseid = AdminDb.GetActionCaseID(time);
            var content = $"🔇 *Gagged* \n" +
                          $"User: {user.Mention}. ({user.Username} | **{user.Id}**)";
            var embed = EmbedGenerator.FooterEmbed(content, $"CASE ID: {caseid[0]}",
                Colours.FailColour, user);
            embed.AddField(x =>
            {
                x.Name = "Moderator";
                x.Value = "Auto Moderator";
                x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = "Reason";
                x.Value = $"{reason}";
                x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = "Message";
                x.Value = $"{message}";
                x.IsInline = false;
            });

            return embed;
        }
    }
}