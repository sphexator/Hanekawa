using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Services.Drop;
using Hanekawa.Database;
using Hanekawa.Extensions;
using Hanekawa.Shared.Command;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Settings
{
    [Name("Drop")]
    [RequireMemberGuildPermissions(Permission.ManageGuild)]
    [RequireBotGuildPermissions(Permission.EmbedLinks)]
    public class Drop : HanekawaModule
    {
        private readonly DropService _drop;
        public Drop(DropService drop) => _drop = drop;

        [Name("Drop")]
        [Command("drop")]
        [Description("Spawns a crate for people to claim. Higher reward then regular crates")]
        public async Task SpawnDrop()
        {
            await Context.Message.TryDeleteMessageAsync();
            await _drop.SpawnAsync(Context);
        }

        [Name("Emote")]
        [Command("de", "dropemote")]
        [Description("Changes claim emote")]
        public async Task DropEmote(LocalCustomEmoji emote)
        {
            await _drop.ChangeEmote(Context.Guild, emote);
            await Context.ReplyAsync($"Changed claim emote to {emote}");
        }

        [Name("Add")]
        [Command("da", "dropadd")]
        [Description("Adds a channel to be eligible for drops")]
        public async Task AddDropChannel(CachedTextChannel channel = null)
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            channel ??= Context.Channel as CachedTextChannel;
            if (channel == null) return;
            try
            {
                await Context.Message.TryDeleteMessageAsync();
                await _drop.AddLootChannel(channel, db);
                await Context.ReplyAsync($"Added {channel.Mention} to loot eligible channels.",
                    Color.Green);
            }
            catch
            {
                await Context.ReplyAsync($"Couldn't add {channel.Mention} to loot eligible channels.",
                    Color.Red);
            }
        }

        [Name("Remove")]
        [Command("dr", "dropremove")]
        [Description("Removes a channel from being eligible for drops")]
        public async Task RemoveDropChannel(CachedTextChannel channel = null)
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            channel ??= Context.Channel as CachedTextChannel;
            if (channel == null) return;
            try
            {
                await Context.Message.TryDeleteMessageAsync();
                await _drop.RemoveLootChannel(channel, db);
                await Context.ReplyAsync($"Removed {channel.Mention} from loot eligible channels.",
                    Color.Green);
            }
            catch
            {
                await Context.ReplyAsync($"Couldn't remove {channel.Mention} from loot eligible channels.",
                    Color.Red);
            }
        }

        [Name("List")]
        [Command("dl", "droplist")]
        [Description("Lists channels that are available for drops")]
        public async Task ListDropChannelsAsync()
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var embed = new LocalEmbedBuilder().WithAuthor(new LocalEmbedAuthorBuilder
                {Name = $"{Context.Guild.Name} Loot channels:", IconUrl = Context.Guild.GetIconUrl()});
            var list = await db.LootChannels.Where(x => x.GuildId == Context.Guild.Id.RawValue).ToListAsync();
            if (list.Count == 0)
            {
                embed.Description = "No loot channels has been added to this server";
            }
            else
            {
                var result = new List<string>();
                foreach (var x in list)
                {
                    var channel = Context.Guild.GetTextChannel(x.ChannelId);
                    if (channel == null) continue;
                    result.Add(channel.Mention);
                }
                embed.Description = string.Join("\n", result);
            }

            await Context.ReplyAsync(embed);
        }
    }
}