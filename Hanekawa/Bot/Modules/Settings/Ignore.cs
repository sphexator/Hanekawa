using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Settings
{
    [Name("Ignore")]
    [RequireBotGuildPermissions(Permission.EmbedLinks)]
    [RequireMemberGuildPermissions(Permission.ManageGuild)]
    public class Ignore : HanekawaModule
    {
        private readonly RequiredChannel _requiredChannel;

        public Ignore(RequiredChannel requiredChannel) => _requiredChannel = requiredChannel;

        [Name("Ignore")]
        [Command("ignore")]
        [Description("Adds or removes a channel from ignore list")]
        public async Task IgnoreChannelAsync(CachedTextChannel channel = null)
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateAdminConfigAsync(Context.Guild);
            channel ??= Context.Channel as CachedTextChannel;
            if (channel == null) return;
            var result = await _requiredChannel.AddOrRemoveChannel(channel, db);
            if (result)
            {
                if (cfg.IgnoreAllChannels)
                    await Context.ReplyAsync($"Added {channel?.Mention} to the ignore list.\n" +
                                             $"Commands are now enabled in {channel?.Mention}",
                        Color.Green);
                else
                    await Context.ReplyAsync($"Added {channel?.Mention} to the ignore list.\n" +
                                             $"Commands are now disabled in {channel?.Mention}",
                        Color.Green);
            }

            if (!result)
            {
                if (cfg.IgnoreAllChannels)
                    await Context.ReplyAsync($"Removed {channel?.Mention} from the ignore list.\n" +
                                             $"Commands are now disabled in {channel?.Mention}",
                        Color.Green);
                else
                    await Context.ReplyAsync($"Removed {channel?.Mention} from the ignore list.\n" +
                                             $"Commands are now enabled in {channel?.Mention}",
                        Color.Green);
            }
        }

        [Name("Toggle")]
        [Command("it", "ignoretoggle")]
        [Description("Toggles whether common commands is only usable in ignored channels or not")]
        public async Task ToggleIgnoreChannelAsync()
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateAdminConfigAsync(Context.Guild);
            if (cfg.IgnoreAllChannels)
            {
                cfg.IgnoreAllChannels = false;
                await Context.ReplyAsync(
                    "Commands are now usable in all channels beside those in the ignore list.");
            }
            else
            {
                cfg.IgnoreAllChannels = true;
                await Context.ReplyAsync("Commands are now only usable in channels on the list.");
            }

            await db.SaveChangesAsync();
        }

        [Name("List")]
        [Command("il", "ignorelist")]
        [Description("Toggles whether common commands is only usable in ignored channels or not")]
        public async Task ListIgnoreChannelsAsync()
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateAdminConfigAsync(Context.Guild);
            var list = await db.IgnoreChannels.Where(x => x.GuildId == Context.Guild.Id.RawValue).ToListAsync();
            string content;
            if (list.Count != 0)
            {
                var channels = new List<string>();
                var toRemove = new List<IgnoreChannel>();
                foreach (var x in list)
                    try
                    {
                        var channel = Context.Guild.GetTextChannel(x.ChannelId);
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
            }
            else
            {
                content = "Commands are usable in every channel";
            }

            var title = cfg.IgnoreAllChannels
                ? "Channel commands are enabled in:"
                : "Channel commands are ignored in:";
            var embed = new LocalEmbedBuilder().Create(content, Context.Colour.Get(Context.Guild.Id.RawValue))
                .WithAuthor(new LocalEmbedAuthorBuilder {IconUrl = Context.Guild.GetIconUrl(), Name = title});
            await Context.ReplyAsync(embed);
        }
    }
}