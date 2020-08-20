using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Extensions;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Qmmands;

namespace Hanekawa.Bot.Modules.Giveaway
{
    public class Giveaway : HanekawaCommandModule
    {
        [Name("Draw")]
        [Command("draw")]
        [Description("Draw(s) winner(s) from a reaction on a message")]
        [Remarks("draw 5 <emote> 5435346235434 #general")]
        [RequireMemberGuildPermissions(Permission.ManageMessages)]
        public async Task DrawWinnerAsync(int winners, LocalCustomEmoji emote, ulong messageId, CachedTextChannel channel = null)
        {
            await Context.Message.TryDeleteMessageAsync();
            var stream = new MemoryStream();
            channel ??= Context.Channel as CachedTextChannel;
            if (channel == null) return;
            if (!(await channel.GetMessageAsync(messageId) is IUserMessage message))
            {
                await Context.ReplyAsync($"Couldn't find a message with that ID in {channel.Mention}",
                    Color.Red);
                return;
            }

            var reactionAmount = GetReactionAmount(message, emote);
            var users = await message.GetReactionsAsync(emote, reactionAmount);
            if (users == null)
            {
                await Context.ReplyAsync(
                    "Couldn't find any users reacting with that emote. You sure this is a emote on this server?",
                    Color.Red);
                return;
            }

            var rnd = new Random();
            var result = users.OrderBy(item => rnd.Next());
            var winnerString = new StringBuilder();

            await using var file = new StreamWriter(stream);
            var nr = 1;
            foreach (var x in result)
            {
                if (nr <= winners) winnerString.AppendLine($"{x}");
                await file.WriteLineAsync($"{nr}: {x.Id.RawValue} - {x.Name}#{x.Discriminator}");
                nr++;
            }

            await file.FlushAsync();
            stream.Seek(0, SeekOrigin.Begin);
            await channel.SendMessageAsync(new LocalAttachment(stream, "participants.txt"),
                $"Drawing winners for giveaway with reaction {emote}:\n{winners}");
        }

        private static int GetReactionAmount(IUserMessage message, LocalCustomEmoji emote)
        {
            message.Reactions.TryGetValue(emote, out var reactionData);
            return reactionData != null ? reactionData.Count : 0;
        }
    }
}