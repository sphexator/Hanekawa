using Discord.WebSocket;
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
                            var guild = _discord.GetGuild(339370914724446208);
                            var role = guild.GetRole(339711429211062273);
                            await user.AddRoleAsync(role);
                            await user.ModifyAsync(x => x.Mute = true);

                            var ch = guild.GetTextChannel(339381104534355970);

                            var content = $"Action: *Gagged* \n" +
                            $"❕ {user.Mention} got *bent*. (**{user.Id}**)\n" +
                            $"Moderator: Auto Moderator \n" +
                            $"Reason: Discord invite link \n" +
                            $"Message: {rawMessage.Content}";
                            var embed = EmbedGenerator.FooterEmbed(content, Colours.FailColour, user);

                            await ch.SendMessageAsync("", false, embed.Build());
                        }
                        else if (rawMessage.Content.IsScamLink() == true)
                        {
                            await rawMessage.DeleteAsync();
                            var guild = _discord.GetGuild(339370914724446208);
                            var role = guild.GetRole(339711429211062273);
                            await user.AddRoleAsync(role);
                            await user.ModifyAsync(x => x.Mute = true);

                            var ch = guild.GetTextChannel(339381104534355970);

                            var content = $"Action: *Gagged* \n" +
                            $"❕ {user.Mention} got *bent*. (**{user.Id}**)\n" +
                            $"Moderator: Auto Moderator \n" +
                            $"Reason: Scam/malicious link \n" +
                            $"Message: {rawMessage.Content}";
                            var embed = EmbedGenerator.FooterEmbed(content, Colours.FailColour, user);

                            await ch.SendMessageAsync("", false, embed.Build());
                        }
                        else if (rawMessage.Content.Length >= 1500)
                        {
                            await rawMessage.DeleteAsync();
                            var guild = _discord.GetGuild(339370914724446208);
                            var role = guild.GetRole(339711429211062273);
                            await user.AddRoleAsync(role);
                            await user.ModifyAsync(x => x.Mute = true);

                            var ch = guild.GetTextChannel(339381104534355970);

                            var content = $"Action: *Gagged* \n" +
                            $"❕ {user.Mention} got *bent*. (**{user.Id}**)\n" +
                            $"Moderator: Auto Moderator \n" +
                            $"Reason: Character count >= 1500 \n" +
                            $"Message: Too Long Didn't Read.";
                            var embed = EmbedGenerator.FooterEmbed(content, Colours.FailColour, user);

                            await ch.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    else return;
                }
                catch
                {

                }
            });
            return Task.CompletedTask;
        }
    }
}
