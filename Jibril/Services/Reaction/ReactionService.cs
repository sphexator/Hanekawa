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
                                var embed = new EmbedBuilder();
                                var author = new EmbedAuthorBuilder();
                                var footer = new EmbedFooterBuilder();

                                if (arg1.Value.Attachments != null)
                                {
                                    var file = arg1.Value.Attachments.Select(x => x.Url).ToString();
                                    if (_AttachmentUrl(file))
                                        embed.ImageUrl = file;
                                }

                                var hour = $"{arg1.Value.Timestamp.DateTime.ToString("HH")}";
                                var minute = $"{arg1.Value.Timestamp.DateTime.ToString("mm")}";
                                var formattedDate = string.Format(new DateTimeFormatProvider(), "{0}", arg1.Value.Timestamp.Date);
                                author.IconUrl = arg1.Value.Author.GetAvatarUrl();
                                author.Name = arg1.Value.Author.Username;
                                footer.Text = $"{formattedDate} at {hour}:{minute}";
                                embed.Description = content;
                                embed.WithAuthor(author);
                                embed.WithFooter(footer);
                                embed.Color = new Color(Colours.DefaultColour);

                                var guild = _discord.GetGuild(339370914724446208);
                                var msgch = guild.GetChannel(365479361207468032) as ITextChannel;

                                await msgch.SendMessageAsync(
                                    $"<:OwO:357977235510263808> {channel.Mention} ID:{arg1.Id}", false, embed.Build());
                            }
                        }
                    }
                }
                catch
                {
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
                    var reactionData = ReactionDb.ReactionData(msgid); // Need to create Db file
                    if (reactionData == null) return;
                    if (reactionData != null)
                        ReactionDb.RemoveReaction(msgid);
                }
            });
            return Task.CompletedTask;
        }

        private static bool _AttachmentUrl(string url)
        {
            if (url.EndsWith(".png")) return true;
            if (url.EndsWith(".jpeg")) return true;
            if (url.EndsWith(".jpg")) return true;
            if (url.EndsWith(".gif")) return true;
            return false;
        }
    }
}