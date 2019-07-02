using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Interactive;
using Qmmands;

namespace Hanekawa.Bot.Modules.Giveaway
{
    public class Giveaway : InteractiveBase
    {
        [Name("Draw")]
        [Command("draw")]
        [Description("Draw(s) winner(s) from a reaction on a message")]
        [Remarks("draw 5 <emote> 5435346235434 #general")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task DrawWinnerAsync(int winners, Emote emote, ulong messageId, SocketTextChannel channel = null)
        {
            await Context.Message.DeleteAsync();
            var stream = new MemoryStream();
            if (channel == null) channel = Context.Channel;
            if (!(await channel.GetMessageAsync(messageId) is IUserMessage message))
            {
                await Context.ReplyAsync($"Couldn't find a message with that ID in {channel.Mention}",
                    Color.Red.RawValue);
                return;
            }

            var reactionAmount = GetReactionAmount(message, emote);
            var users = await message.GetReactionUsersAsync(emote, reactionAmount).FlattenAsync();
            if (users == null)
            {
                await Context.ReplyAsync(
                    "Couldn't find any users reacting with that emote. You sure this is a emote on this server?",
                    Color.Red.RawValue);
                return;
            }
            var rnd = new Random();
            var result = users.OrderBy(item => rnd.Next());
            var winnerString = new StringBuilder();

            using (var file = new StreamWriter(stream))
            {
                var nr = 1;
                foreach (var x in result)
                {
                    if (nr <= winners) winnerString.AppendLine($"{x}");
                    await file.WriteLineAsync($"{nr}: {x.Id} - {x.Username}#{x.Discriminator}");
                    nr++;
                }

                await file.FlushAsync();
                stream.Seek(0, SeekOrigin.Begin);
                await channel.SendFileAsync(stream, "participants.txt",
                    $"Drawing winners for giveaway with reaction {emote}:\n{winners}");
            }
        }

        private static int GetReactionAmount(IUserMessage message, Emote emote)
        {
            message.Reactions.TryGetValue(emote, out var reactionData);
            return reactionData.ReactionCount;
        }
    }
}
