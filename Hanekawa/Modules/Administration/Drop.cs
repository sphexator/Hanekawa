using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Loot;
using Microsoft.EntityFrameworkCore;

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
            await _lootCrates.SpawnCrateAsync(ch, Context.User as SocketGuildUser);
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

        [Command("list", RunMode = RunMode.Async)]
        [Summary("Lists channels that're available for drops")]
        public async Task ListDropChannelsAsync()
        {
            using (var db = new DbService())
            {
                var embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder { Name = $"{Context.Guild.Name} Loot channels:", IconUrl = Context.Guild.IconUrl },
                    Color = Color.Purple
                };
                var list = await db.LootChannels.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                if (list.Count != 0)
                {
                    var result = new List<string>();
                    foreach (var x in list)
                    {
                        result.Add(Context.Guild.GetTextChannel(x.ChannelId).Mention);
                    }

                    embed.Description = string.Join("\n", result);
                    await ReplyAsync(null, false, embed.Build());
                    return;
                }

                embed.Description = "No loot channels has been added to this server";
                await ReplyAsync(null, false, embed.Build());
            }
        }
    }
}