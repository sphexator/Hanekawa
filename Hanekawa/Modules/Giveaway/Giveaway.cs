using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;

namespace Hanekawa.Modules.Giveaway
{
    public class Giveaway : InteractiveBase
    {
        [Command("draw", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        [Summary("Draw(s) winner(s) from a reaction on a message (EMOTE MUST BE ON THE SERVER)")]
        public async Task DrawWinnerAsync(int draw, Emote emote, ulong messageId, ITextChannel channel = null)
        {
            await Context.Message.DeleteAsync();
            var stream = new MemoryStream();
            if (channel == null) channel = Context.Channel as ITextChannel;
            if (!(await channel.GetMessageAsync(messageId) is IUserMessage message))
            {
                await Context.ReplyAsync($"Couldn't find a message with that ID in {channel.Mention}",
                    Color.Red.RawValue);
                return;
            }

            var reactionAmount = GetReactionAmount(message, emote);
            var users = await message.GetReactionUsersAsync(emote, reactionAmount).FlattenAsync();
            if (users == null)
                await Context.ReplyAsync("Couldn't find any users reacting with that emote. You sure this is a emote on this server?",
                    Color.Red.RawValue);
            var rnd = new Random();
            var result = users.OrderBy(item => rnd.Next());
            string winners = null;

            using (var file = new StreamWriter(stream))
            {
                var nr = 1;
                foreach (var x in result)
                {
                    if (nr <= draw) winners += $"{x}\n";
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