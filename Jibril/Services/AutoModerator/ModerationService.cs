using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
using Jibril.Services.Logging;
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

            _discord.MessageReceived += Filter;
            _discord.MessageReceived += PerspectiveApi;
        }

        private Task Filter(SocketMessage rawMessage)
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
                        {
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
                                return;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                return;
                            }
                        }
                        if (rawMessage.Content.IsGoogleLink())
                        {
                            await rawMessage.DeleteAsync();
                        }
                        if (rawMessage.Content.IsScamLink())
                        {
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
                                return;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                return;
                            }
                        }
                        if (rawMessage.Content.Length >= 1500)
                        {
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
                                return;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                return;
                            }
                        }
                        /*
                        var userdata = DatabaseService.UserData(rawMessage.Author).FirstOrDefault();
                        if (userdata.Level <= 3 && rawMessage.Content.IsUrl())
                        {
                            await rawMessage.DeleteAsync();
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

        private Task PerspectiveApi(SocketMessage msg)
        {
            var _ = Task.Run(() =>
            {
                if (!(msg is SocketUserMessage message)) return;
                if (message.Source != MessageSource.User) return;
                try
                {
                    var content = msg.Content;
                    var emote = new Regex("((:)([a-z]).*?(:))",
                        RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var emoteLeftover = new Regex("((<)([0-9]).*?(>))",
                        RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var mention = new Regex("((<@)([0-9]).*?(>))|((<@!)([0-9]).*?(>))",
                        RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var emoteFilter = emote.Replace(content, "");
                    var emoteFilterv2 = emoteLeftover.Replace(emoteFilter, "");
                    var mentionFilter = mention.Replace(emoteFilterv2, "");
                    if (mentionFilter.IsNullOrWhiteSpace()) return;
                    var request = new AnalyzeCommentRequest(mentionFilter);

                    var response = SendNudes(request);
                    var score = response.AttributeScores.TOXICITY.SummaryScore.Value;
                    var analyze = CalculateNudeScore(score, msg.Author).FirstOrDefault();
                    AdminDb.AddToxicityValue(analyze.ToxicityValue, analyze.Toxicityavg, msg.Author);
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

        private List<ToxicityList> CalculateNudeScore(double score, IUser user)
        {
            var userdata = DatabaseService.UserData(user).FirstOrDefault();
            var calculate = userdata.Toxicityvalue + score;
            var avg = calculate / (userdata.Toxicitymsgcount + 1);
            var result = new List<ToxicityList>
            {
                new ToxicityList
                {
                    ToxicityValue = calculate,
                    Toxicitymsgcount = userdata.Toxicitymsgcount + 1,
                    Toxicityavg = avg
                }
            };
            return result;
        }

        private static EmbedBuilder AutoModResponse(IUser user, string reason, string message, string length = null)
        {
            var time = DateTime.Now;
            AdminDb.AddActionCase(user, time);
            var caseid = AdminDb.GetActionCaseID(time);

            var author = new EmbedAuthorBuilder
            {
                IconUrl = user.GetAvatarUrl(),
                Name = $"Case {caseid[0]} | {ActionType.Gagged} | {user.Username}#{user.DiscriminatorValue}"
            };
            var footer = new EmbedFooterBuilder
            {
                Text = $"ID:{user.Id} | {DateTime.UtcNow}"
            };
            var embed = new EmbedBuilder
            {
                Color = new Color(Colours.FailColour),
                Author = author,
                Footer = footer
            };
            embed.AddField(x =>
            {
                x.Name = "User";
                x.Value = $"{user.Mention}";
                x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = "Moderator";
                x.Value = $"Auto Moderator";
                x.IsInline = true;
            });
            if (length != null)
                embed.AddField(x =>
                {
                    x.Name = "Length";
                    x.Value = $"{length}";
                    x.IsInline = true;
                });
            embed.AddField(x =>
            {
                x.Name = "Reason";
                x.Value = $"{reason}";
                x.IsInline = true;
            });
            if (message.Length < 1000)
                embed.AddField(x =>
                {
                    x.Name = "Message";
                    x.Value = $"{message}";
                    x.IsInline = false;
                });
            return embed;
        }
    }

    public class ToxicityList
    {
        public double ToxicityValue { get; set; }
        public int Toxicitymsgcount { get; set; }
        public double Toxicityavg { get; set; }
    }
}