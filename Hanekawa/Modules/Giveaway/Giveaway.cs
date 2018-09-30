using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Giveaway
{
    public class Giveaway : InteractiveBase
    {
        [Command("draw", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        public async Task DrawWinnerAsync(int draw, Emote emote, ulong messageId)
        {
            await Context.Message.DeleteAsync();
            var stream = new MemoryStream();
            var channel = Context.Channel as ITextChannel;
            var message = (await channel.GetMessageAsync(messageId, CacheMode.AllowDownload)) as IUserMessage;
            var users = await message.GetReactionUsersAsync(emote, message.Reactions.Count).FlattenAsync();

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
                await Context.Channel.SendFileAsync(stream, "participants.txt", $"Drawing winners for giveaway with reaction {emote}:\n{winners}");
            }
        }
    }
}
