using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Extensions;

namespace Jibril.Services.Reaction
{
    public class ReactionService
    {
        private readonly DiscordSocketClient _discord;
        private IServiceProvider _provider;

        public ReactionService(DiscordSocketClient discord, IServiceProvider provider)
        {
            _discord = discord;
            _provider = provider;

            _discord.ReactionAdded += _discord_ReactionAdded;
            _discord.ReactionRemoved += _discord_ReactionRemoved;
        }

        private Task _discord_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2,
            SocketReaction arg3)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    var channel = arg2 as ITextChannel;
                    if (arg3.Emote.Name == "OwO" && arg2.Id != 364096978545803265 && arg2.Id != 365479361207468032 &&
                        channel.IsNsfw == false)
                    {
                        var msgid = arg1.Id.ToString();
                        var chid = arg2.Id.ToString();
                        var reactionData = ReactionDb.ReactionData(msgid).FirstOrDefault();
                        if (reactionData == null) ReactionDb.InsertReactionMessage(msgid, chid, 1);
                        if (reactionData != null && reactionData.Sent == "no")
                        {
                            ReactionDb.AddReaction(msgid);
                            var counter = reactionData.Counter + 1;
                            if (counter == 4)
                            {
                                ReactionDb.ReactionMsgPosted(msgid);
                                var content = arg3.Message.Value.Content;
                                var author = new EmbedAuthorBuilder
                                {
                                    IconUrl = arg1.Value.Author.GetAvatarUrl(),
                                    Name = arg1.Value.Author.Username
                                };
                                var footer = new EmbedFooterBuilder {Text = $"{channel.Name}"};
                                var embed = new EmbedBuilder
                                {
                                    Description = content,
                                    Color = new Color(Colours.DefaultColour),
                                    Author = author,
                                    Footer = footer,
                                    Timestamp = arg1.Value.Timestamp
                                };
                                if (arg3.Message.Value.Content != null) embed.Description = arg3.Message.Value.Content;
                                if (arg1.Value.Attachments.Count > 0)
                                {
                                    var image = arg1.Value.Attachments.First(x => x.Url != null).Url;
                                    embed.ImageUrl = image;
                                }
                                var guild = _discord.GetGuild(339370914724446208);
                                if (!(guild.GetChannel(365479361207468032) is ITextChannel msgch)) return;
                                await msgch.SendMessageAsync("", false, embed.Build());
                            }
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            });
            return Task.CompletedTask;
        }

        private Task _discord_ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2,
            SocketReaction arg3)
        {
            var _ = Task.Run(() =>
            {
                if (arg3.Emote.Name == "OwO" && arg2.Id != 364096978545803265)
                {
                    var msgid = arg1.Id.ToString();
                    var reactionData = ReactionDb.ReactionData(msgid);
                    if (reactionData == null) return;
                    ReactionDb.RemoveReaction(msgid);
                }
            });
            return Task.CompletedTask;
        }
    }
}