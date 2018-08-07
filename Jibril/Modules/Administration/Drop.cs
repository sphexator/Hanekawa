using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Services.Loot;

namespace Hanekawa.Modules.Administration
{
    [Group("drop")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireContext(ContextType.Guild)]
    public class Drop : InteractiveBase
    {
        private readonly LootCrates _lootCrates;
        public Drop(LootCrates lootCrates)
        {
            _lootCrates = lootCrates;
        }

        [Command(RunMode = RunMode.Async)]
        [Summary("Spawns a crate for people to claim. Higher reward then regular crates")]
        public async Task SpawnDrop()
        {
            await Context.Message.DeleteAsync();
            if (!(Context.Channel is SocketTextChannel ch)) return;
            await _lootCrates.SpawnCrate(ch, Context.User as SocketGuildUser);
        }

        [Command("Add", RunMode = RunMode.Async)]
        [Summary("Adds a channel to be eligible for drops")]
        public async Task AddDropChannel(ITextChannel channel = null)
        {
            try
            {
                if (channel == null) channel = Context.Channel as ITextChannel;
                await _lootCrates.AddLootChannelAsync(channel as SocketTextChannel);
                await Context.Message.DeleteAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Added {channel.Mention} to loot eligable channels.", Color.Green.RawValue)
                        .Build());
            }
            catch
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Couldn't add {channel.Mention} to loot eligable channels.", Color.Red.RawValue)
                        .Build());
            }
        }

        [Command("remove", RunMode = RunMode.Async)]
        [Summary("Removes a channel from being eligible for drops")]
        public async Task RemoveDropChannel(ITextChannel channel = null)
        {
            try
            {
                if (channel == null) channel = Context.Channel as ITextChannel;
                await _lootCrates.RemoveLootChannelAsync(channel as SocketTextChannel);
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Removed {channel.Mention} from loot eligable channels.", Color.Green.RawValue)
                        .Build());
            }
            catch
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Couldn't remove {channel.Mention} from loot eligable channels.", Color.Red.RawValue)
                        .Build());
            }
        }
    }
}
