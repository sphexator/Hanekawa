using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Extensions.Embed;
using Hanekawa.Services.Drop;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Administration
{
    [RequireUserPermission(GuildPermission.ManageGuild)]
    [RequireContext(ContextType.Guild)]
    public class Drop : InteractiveBase
    {
        private readonly DropData _dropData;
        private readonly DropService _dropService;

        public Drop(DropService dropService, DropData dropData)
        {
            _dropService = dropService;
            _dropData = dropData;
        }

        [Name("Drop")]
        [Command("drop", RunMode = RunMode.Async)]
        [Summary("Spawns a crate for people to claim. Higher reward then regular crates")]
        [Remarks("h.drop")]
        public async Task SpawnDrop()
        {
            await Context.Message.DeleteAsync();
            if (!(Context.Channel is SocketTextChannel ch)) return;
            await _dropService.SpawnCrateAsync(ch, Context.User as SocketGuildUser);
        }

        [Name("Drop add")]
        [Command("drop Add", RunMode = RunMode.Async)]
        [Summary("Adds a channel to be eligible for drops")]
        [Remarks("h.drop add #general")]
        public async Task AddDropChannel(ITextChannel channel = null)
        {
            try
            {
                if (channel == null) channel = Context.Channel as ITextChannel;
                await _dropData.AddLootChannelAsync(channel as SocketTextChannel);
                await Context.Message.DeleteAsync();
                await Context.ReplyAsync($"Added {channel.Mention} to loot eligible channels.",
                    Color.Green.RawValue);
            }
            catch
            {
                await Context.ReplyAsync($"Couldn't add {channel.Mention} to loot eligible channels.",
                    Color.Red.RawValue);
            }
        }

        [Name("Drop remove")]
        [Command("drop remove", RunMode = RunMode.Async)]
        [Summary("Removes a channel from being eligible for drops")]
        [Remarks("h.drop remove #general")]
        public async Task RemoveDropChannel(ITextChannel channel = null)
        {
            try
            {
                if (channel == null) channel = Context.Channel as ITextChannel;
                await _dropData.RemoveLootChannelAsync(channel as SocketTextChannel);
                await Context.ReplyAsync($"Removed {channel.Mention} from loot eligible channels.",
                    Color.Green.RawValue);
            }
            catch
            {
                await Context.ReplyAsync($"Couldn't remove {channel.Mention} from loot eligible channels.",
                    Color.Red.RawValue);
            }
        }

        [Name("Drop list")]
        [Command("drop list", RunMode = RunMode.Async)]
        [Summary("Lists channels that are available for drops")]
        [Remarks("h.drop list")]
        public async Task ListDropChannelsAsync()
        {
            using (var db = new DbService())
            {
                var embed = new EmbedBuilder().CreateDefault(Context.Guild.Id).WithAuthor(new EmbedAuthorBuilder
                    {Name = $"{Context.Guild.Name} Loot channels:", IconUrl = Context.Guild.IconUrl});
                var list = await db.LootChannels.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                if (list.Count == 0)
                {
                    embed.Description = "No loot channels has been added to this server";
                }
                else
                {
                    var result = new List<string>();
                    foreach (var x in list) result.Add(Context.Guild.GetTextChannel(x.ChannelId).Mention);
                    embed.Description = string.Join("\n", result);
                }

                await Context.ReplyAsync(embed);
            }
        }
    }
}