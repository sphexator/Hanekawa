using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using System.Text;
using Newtonsoft.Json;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Extensions;
using Jibril.Modules.Administration.Services;
using Jibril.Services.AutoModerator.Perspective.Models;
using Jibril.Services.Common;

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
            _discord.MessageReceived += PerspectiveApi;
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
                    var staffCheck = user.GuildPermissions.ManageMessages;
                    if (staffCheck != true)
                    {
                        if (rawMessage.Content.IsDiscordInvite())
                            try
                            {
                                await rawMessage.DeleteAsync();

                                var time = DateTime.Now;
                                AdminDb.AddActionCase(user, time);
                                var caseid = AdminDb.GetActionCaseID(time);

                                var guild = _discord.GetGuild(339370914724446208);
                                var role = guild.Roles.FirstOrDefault(r => r.Name == "Mute");
                                await user.AddRoleAsync(role);
                                await user.ModifyAsync(x => x.Mute = true);

                                var ch = guild.GetTextChannel(339381104534355970);

                                var content = $"🔇 *Gagged* \n" +
                                              $"User: {user.Mention}. (**{user.Id}**)";
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
                                    x.Value = "Discord invite link";
                                    x.IsInline = true;
                                });
                                embed.AddField(x =>
                                {
                                    x.Name = "Message";
                                    x.Value = $"{rawMessage.Content}";
                                    x.IsInline = false;
                                });

                                await ch.SendMessageAsync("", false, embed.Build());
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        if (rawMessage.Content.IsScamLink())
                            try
                            {
                                await rawMessage.DeleteAsync();

                                var time = DateTime.Now;
                                AdminDb.AddActionCase(user, time);
                                var caseid = AdminDb.GetActionCaseID(time);

                                var guild = _discord.GetGuild(339370914724446208);
                                var role = guild.Roles.FirstOrDefault(r => r.Name == "Mute");
                                await user.AddRoleAsync(role);
                                await user.ModifyAsync(x => x.Mute = true);

                                var ch = guild.GetTextChannel(339381104534355970);

                                var content = $"🔇 *Gagged* \n" +
                                              $"User: {user.Mention}. (**{user.Id}**)";
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
                                    x.Value = "Scam/malicious link";
                                    x.IsInline = true;
                                });
                                embed.AddField(x =>
                                {
                                    x.Name = "Message";
                                    x.Value = $"{rawMessage.Content}";
                                    x.IsInline = false;
                                });

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

                                var time = DateTime.Now;
                                AdminDb.AddActionCase(user, time);
                                var caseid = AdminDb.GetActionCaseID(time);

                                var guild = _discord.GetGuild(339370914724446208);
                                var role = guild.Roles.FirstOrDefault(r => r.Name == "Mute");
                                await user.AddRoleAsync(role);
                                await user.ModifyAsync(x => x.Mute = true);

                                var ch = guild.GetTextChannel(339381104534355970);

                                var content = $"🔇 *Gagged* \n" +
                                              $"User: {user.Mention}. (**{user.Id}**)";
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
                                    x.Value = "Character count >= 1500";
                                    x.IsInline = true;
                                });
                                embed.AddField(x =>
                                {
                                    x.Name = "Message";
                                    x.Value = $"Too Long Didn't Read.";
                                    x.IsInline = false;
                                });

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
                if (msg.Author.IsBot != true) return;
                var content = msg.Content;
                var request = new AnalyzeCommentRequest(content);

                var response = SendNudes(request);
                var score = response.AttributeScores.TOXICITY.SummaryScore.Value;
                Console.WriteLine($"{DateTime.Now} | TOXICITY SERVICE | {msg.Author.Id} | {msg.Author.Username} | Toxicity score:{score}");
            });
            return Task.CompletedTask;
        }

        private AnalyzeCommentResponse SendNudes(AnalyzeCommentRequest request)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var output = JsonConvert.SerializeObject(request);
                Console.WriteLine(output);
                var response = client.PostAsync($"https://commentanalyzer.googleapis.com/v1alpha1/comments:analyze?key={Token.key}", content).Result;
                response.EnsureSuccessStatusCode();
                var data = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<AnalyzeCommentResponse>(data);
                return result;
            }
        }
    }
}