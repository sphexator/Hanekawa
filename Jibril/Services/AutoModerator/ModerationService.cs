﻿using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Jibril.Extensions;
using Jibril.Data.Variables;
using System.Linq;
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
                        if (rawMessage.Content.IsDiscordInvite() == true)
                        {
                            await rawMessage.DeleteAsync();
                            var caseid = CaseNumberGenerator.InsertCaseID(user);
                            var guild = _discord.GetGuild(339370914724446208);
                            var role = guild.Roles.FirstOrDefault(r => r.Name == "Mute");
                            await user.AddRoleAsync(role);
                            await user.ModifyAsync(x => x.Mute = true);

                            var ch = guild.GetTextChannel(339381104534355970);

                            var content = $"🔇 *Gagged* \n" +
                            $"User: {user.Mention}. (**{user.Id}**)";
                            var embed = EmbedGenerator.FooterEmbed(content, $"CASE ID: {caseid[0]}", Colours.FailColour, user);
                            embed.AddField(x =>
                            {
                                x.Name = "Auto Moderator";
                                x.Value = "N/A";
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
                        if (rawMessage.Content.IsScamLink() == true)
                        {
                            await rawMessage.DeleteAsync();
                            var caseid = CaseNumberGenerator.InsertCaseID(user);
                            var guild = _discord.GetGuild(339370914724446208);
                            var role = guild.Roles.FirstOrDefault(r => r.Name == "Mute");
                            await user.AddRoleAsync(role);
                            await user.ModifyAsync(x => x.Mute = true);

                            var ch = guild.GetTextChannel(339381104534355970);

                            var content = $"🔇 *Gagged* \n" +
                            $"User: {user.Mention}. (**{user.Id}**)";
                            var embed = EmbedGenerator.FooterEmbed(content, $"CASE ID: {caseid[0]}", Colours.FailColour, user);
                            embed.AddField(x =>
                            {
                                x.Name = "Auto Moderator";
                                x.Value = "N/A";
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
                        if (rawMessage.Content.Length >= 1500)
                        {
                            await rawMessage.DeleteAsync();
                            var caseid = CaseNumberGenerator.InsertCaseID(user);
                            var guild = _discord.GetGuild(339370914724446208);
                            var role = guild.Roles.FirstOrDefault(r => r.Name == "Mute");
                            await user.AddRoleAsync(role);
                            await user.ModifyAsync(x => x.Mute = true);

                            var ch = guild.GetTextChannel(339381104534355970);

                            var content = $"🔇 *Gagged* \n" +
                            $"User: {user.Mention}. (**{user.Id}**)";
                            var embed = EmbedGenerator.FooterEmbed(content, $"CASE ID: {caseid[0]}", Colours.FailColour, user);
                            embed.AddField(x =>
                            {
                                x.Name = "Auto Moderator";
                                x.Value = "N/A";
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
                    }
                }
                catch
                {

                }
            });
            return Task.CompletedTask;
        }
    }
}
