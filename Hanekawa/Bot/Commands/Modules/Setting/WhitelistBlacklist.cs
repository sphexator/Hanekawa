using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Entities.Color;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules.Setting
{
    [Name("Ignore")]
    [Description("Configure channels where commands are usable")]
    [Group("ignore")]
    public class WhitelistBlacklist : HanekawaCommandModule
    {
        private readonly CacheService _cache;
        public WhitelistBlacklist(CacheService cache) => _cache = cache;

        [Name("Add/Remove Ignore Channel")]
        [Command("add", "remove")]
        [Description("Adds or removes a channel from ignore list")]
        public async Task<DiscordCommandResult> IgnoreChannelAsync(ITextChannel channel = null)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateAdminConfigAsync(Context.Guild);
            channel ??= Context.Channel;
            if (channel == null) return null;
            var result = await _cache.AddOrRemoveChannel(channel, db);
            if (!result)
                return cfg.IgnoreAllChannels
                    ? Reply($"Removed {channel.Mention} from the ignore list.\n" +
                            $"Commands are now disabled in {channel.Mention}",
                        HanaBaseColor.Ok())
                    : Reply($"Removed {channel.Mention} from the ignore list.\n" +
                            $"Commands are now enabled in {channel.Mention}",
                        HanaBaseColor.Ok());
            if (cfg.IgnoreAllChannels)
                return Reply($"Added {channel.Mention} to the ignore list.\n" +
                             $"Commands are now enabled in {channel.Mention}",
                    HanaBaseColor.Ok());
            return Reply($"Added {channel.Mention} to the ignore list.\n" +
                         $"Commands are now disabled in {channel.Mention}",
                HanaBaseColor.Ok());

        }

        [Name("Toggle")]
        [Command("toggle")]
        [Description("Toggles whether common commands is only usable in ignored channels or not")]
        public async Task<DiscordCommandResult> ToggleIgnoreChannelAsync()
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateAdminConfigAsync(Context.Guild);
            if (cfg.IgnoreAllChannels)
            {
                cfg.IgnoreAllChannels = false;
                await db.SaveChangesAsync();
                return Reply(
                    "Commands are now usable in all channels beside those in the ignore list.", HanaBaseColor.Ok());
            }
            cfg.IgnoreAllChannels = true;
            await db.SaveChangesAsync();
            return Reply("Commands are now only usable in channels on the list.", HanaBaseColor.Ok());
            
        }

        [Name("List")]
        [Command("list")]
        [Description("Toggles whether common commands is only usable in ignored channels or not")]
        public async Task<DiscordCommandResult> ListIgnoreChannelsAsync()
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateAdminConfigAsync(Context.Guild);
            var list = await db.IgnoreChannels.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
            string content;
            if (list.Count != 0)
            {
                var channels = new List<string>();
                var toRemove = new List<IgnoreChannel>();
                foreach (var x in list)
                    try
                    {
                        var channel = Context.Guild.GetChannel(x.ChannelId) as ITextChannel;
                        if (channel == null)
                        {
                            toRemove.Add(x);
                            continue;
                        }

                        channels.Add(channel.Mention);
                    }
                    catch
                    {
                        /* ignored todo: maybe handle this better? channel ignore list */
                    }

                content = string.Join("\n", channels);
                db.IgnoreChannels.RemoveRange(toRemove);
                await db.SaveChangesAsync();
            }
            else
            {
                content = "Commands are usable in every channel";
            }

            var title = cfg.IgnoreAllChannels
                ? "Channel commands are enabled in:"
                : "Channel commands are ignored in:";
            return Reply(new LocalEmbed
            {
                Color = Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId),
                Author = new LocalEmbedAuthor {IconUrl = Context.Guild.GetIconUrl(), Name = title},
                Description = content
            });
        }
    }
}