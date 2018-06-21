using Discord;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Extensions;
using Jibril.Services.AutoModerator.Perspective.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ActionType = Jibril.Services.Log.ActionType;

namespace Jibril.Services.AutoModerator
{
    public class ModerationService
    {
        private readonly IConfiguration _config;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _provider;

        public ModerationService(DiscordSocketClient discord, IServiceProvider provider, IConfiguration config)
        {
            _discord = discord;
            _provider = provider;
            _config = config;

            PerspectiveToken = _config["perspective"];

            _discord.MessageReceived += Filter;
            _discord.MessageReceived += PerspectiveApi;
            _discord.UserJoined += _discord_UserJoined;
        }

        private static string PerspectiveToken { get; set; }

        private Task _discord_UserJoined(SocketGuildUser user)
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

        private static EmbedBuilder EmbedBuilder(string x, IGuildUser u)
        {
            var txt = FieldBuilders(x, u);
            var author = new EmbedAuthorBuilder
            {
                IconUrl = u.GetAvatarUrl(),
                Name = u.Username
            };
            var embed = new EmbedBuilder
            {
                Color = new Color(Colours.DefaultColour),
                Title = "Suspicious user",
                Fields = txt,
                Author = author
            };

            return embed;
        }

        private static List<EmbedFieldBuilder> FieldBuilders(string xx, IGuildUser usr)
        {
            var x = FilterString(xx);
            var fields = new List<EmbedFieldBuilder>();
            var tag = new EmbedFieldBuilder
            {
                Name = "Name",
                Value = usr.Mention,
                IsInline = true
            };
            var id = new EmbedFieldBuilder
            {
                Name = "User ID",
                Value = usr.Id,
                IsInline = true
            };
            var reason = new EmbedFieldBuilder
            {
                Name = "Reason",
                Value = $"{x[3]}",
                IsInline = true
            };
            var proof = new EmbedFieldBuilder
            {
                Name = "Proof",
                Value = $"{x[4]}",
                IsInline = true
            };
            fields.Add(tag);
            fields.Add(id);
            fields.Add(reason);
            fields.Add(proof);

            return fields;
        }

        private static string[] FilterString(string x)
        {
            var s = x.Split(",", StringSplitOptions.RemoveEmptyEntries);
            return s;
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

                        if (rawMessage.Content.IsPornLink() && ((ITextChannel) rawMessage.Channel).IsNsfw != true)
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

        private Task PerspectiveApi(SocketMessage msg)
        {
            var _ = Task.Run(async () =>
            {
                if (!(msg is SocketUserMessage message)) return;
                if (message.Source != MessageSource.User) return;
                try
                {
                    using (var db = new hanekawaContext())
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
                        var analyze = (await CalculateNudeScore(score, msg.Author)).FirstOrDefault();
                        var user = await db.GetOrCreateUserData(msg.Author);
                        user.Toxicityvalue = analyze.ToxicityValue;
                        user.Toxicitymsgcount = user.Toxicitymsgcount + 1;
                        user.Toxicityavg = analyze.Toxicityavg;
                        await db.SaveChangesAsync();
                        Console.WriteLine(
                            $"{DateTime.Now.ToLongTimeString()} | TOXICITY SERVICE | {msg.Author.Id} | Toxicity score:{score} | {msg.Author.Username}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Toxicity failed");
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
                    .PostAsync(
                        $"https://commentanalyzer.googleapis.com/v1alpha1/comments:analyze?key={PerspectiveToken}",
                        content).Result;
                response.EnsureSuccessStatusCode();
                var data = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<AnalyzeCommentResponse>(data);
                return result;
            }
        }

        private static async Task<IEnumerable<ToxicityList>> CalculateNudeScore(double score, IUser user)
        {
            using (var db = new hanekawaContext())
            {
                var userdata = await db.GetOrCreateUserData(user);
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
        }

        private static EmbedBuilder AutoModResponse(IUser user, string reason, string message, string length = null)
        {
            using (var db = new hanekawaContext())
            {
                var caseid = db.GetOrCreateCaseId(user, DateTime.Now);

                var author = new EmbedAuthorBuilder
                {
                    IconUrl = user.GetAvatarUrl(),
                    Name = $"Case {caseid} | {ActionType.Gagged} | {user.Username}#{user.DiscriminatorValue}"
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
    }

    public class ToxicityList
    {
        public double ToxicityValue { get; set; }
        public int Toxicitymsgcount { get; set; }
        public double Toxicityavg { get; set; }
    }
}