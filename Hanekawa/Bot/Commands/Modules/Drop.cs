using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Bot.Service.Drop;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Entities;
using Hanekawa.Entities.Color;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules
{
    [Name("Drop")]
    [Description("Commands for drop module")]
    [Group("Drop")]
    [RequireAuthorGuildPermissions(Permission.ManageGuild)]
    [RequireBotGuildPermissions(Permission.EmbedLinks)]
    public class Drop : HanekawaCommandModule
    {
        [Name("Drop")]
        [Command("drop")]
        [Description("Spawns a crate for people to claim. Higher reward then regular crates")]
        public async Task SpawnDrop()
        {
            await Context.Message.TryDeleteMessageAsync();
            var drop = Context.Services.GetRequiredService<DropService>();
            await drop.SpawnAsync(Context.GuildId, Context.Channel, Context.Message, DropType.Special);
        }

        public class DropAdmin : Drop, IModuleSetting
        {
            [Name("Emote")]
            [Command("emote")]
            [Description("Changes drop claim emote")]
            public async Task DropEmote(IGuildEmoji emote)
            {
                var cache = Context.Services.GetRequiredService<CacheService>();
                cache.AddOrUpdateEmote(EmoteType.Drop, Context.GuildId, emote);
                await Reply($"Changed claim emote to {emote}", HanaBaseColor.Ok());
            }

            [Name("Add")]
            [Command("add")]
            [Description("Adds a channel to be eligible for drops")]
            public async Task AddDropChannel(ITextChannel channel = null)
            {
                channel ??= Context.Channel;
                if (channel == null) return;
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cache = Context.Services.GetRequiredService<CacheService>();
                try
                {
                    await Context.Message.TryDeleteMessageAsync();
                    var dbChannel = await db.LootChannels.FindAsync(Context.GuildId, channel.Id);
                    if (dbChannel != null)
                    {
                        await Reply($"{channel.Mention} is already added as a drop channel!", HanaBaseColor.Bad());
                        return;
                    }

                    await db.LootChannels.AddAsync(new LootChannel {GuildId = Context.GuildId, ChannelId = channel.Id});
                    await db.SaveChangesAsync();
                    cache.AddDropChannel(Context.GuildId, channel.Id);
                    await Reply($"Added {channel.Mention} to loot eligible drop channels.",
                        HanaBaseColor.Ok());
                }
                catch
                {
                    await Reply($"Couldn't add {channel.Mention} to loot eligible drop channels.",
                        HanaBaseColor.Bad());
                }
            }

            [Name("Remove")]
            [Command("remove")]
            [Description("Removes a channel from being eligible for drops")]
            public async Task RemoveDropChannel(ITextChannel channel = null)
            {
                channel ??= Context.Channel;
                if (channel == null) return;
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cache = Context.Services.GetRequiredService<CacheService>();
                try
                {
                    await Context.Message.TryDeleteMessageAsync();
                    var dbChannel = await db.LootChannels.FindAsync(Context.GuildId, channel.Id);
                    if (dbChannel == null)
                    {
                        await Reply($"{channel.Mention} is not a drop eligible drop channel!", HanaBaseColor.Bad());
                        return;
                    }

                    db.Remove(dbChannel);
                    await db.SaveChangesAsync();
                    cache.RemoveDropChannel(Context.GuildId, channel.Id);
                    await Reply($"Removed {channel.Mention} from loot eligible drop channels.",
                        HanaBaseColor.Ok());
                }
                catch
                {
                    await Reply($"Couldn't remove {channel.Mention} from loot eligible drop channels.",
                        HanaBaseColor.Bad());
                }
            }

            [Name("List")]
            [Command("list")]
            [Description("Lists channels that are available for drops")]
            public async Task ListDropChannelsAsync()
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var embed = new LocalEmbed().WithAuthor(new LocalEmbedAuthor
                    {Name = $"{Context.Guild.Name} Loot channels:", IconUrl = Context.Guild.GetIconUrl()});
                var list = await db.LootChannels.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                if (list.Count == 0)
                {
                    embed.Description = "No channels has been added for drops.";
                }
                else
                {
                    var result = new List<string>();
                    foreach (var x in list)
                    {
                        var channel = Context.Guild.GetChannel(x.ChannelId) as ITextChannel;
                        if (channel == null) continue;
                        result.Add(channel.Mention);
                    }

                    embed.Description = string.Join("\n", result);
                }

                await Reply(embed);
            }
        }
    }
}