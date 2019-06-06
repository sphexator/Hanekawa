using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Core.Interactive;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Extensions.Embed;
using Microsoft.EntityFrameworkCore;
using Qmmands;

namespace Hanekawa.Bot.Modules.Settings
{
    [Name("Ignore")]
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public class Ignore : InteractiveBase
    {
        private readonly RequiredChannel _requiredChannel;

        public Ignore(RequiredChannel requiredChannel) => _requiredChannel = requiredChannel;

        [Name("Ignore")]
        [Command("ignore")]
        [Description("Adds or removes a channel from ignore list")]
        [Remarks("ignore #general")]
        public async Task IgnoreChannelAsync(SocketTextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateAdminConfigAsync(Context.Guild);
                if (channel == null) channel = Context.Channel;
                var result = await _requiredChannel.AddOrRemoveChannel(channel, db);
                if (result)
                {
                    if (cfg.IgnoreAllChannels)
                        await Context.ReplyAsync($"Added {channel?.Mention} to the ignore list.\n" +
                                                 $"Commands are now enabled in {channel?.Mention}",
                            Color.Green.RawValue);
                    else
                        await Context.ReplyAsync($"Added {channel?.Mention} to the ignore list.\n" +
                                                 $"Commands are now disabled in {channel?.Mention}",
                            Color.Green.RawValue);
                }

                if (!result)
                {
                    if (cfg.IgnoreAllChannels)
                        await Context.ReplyAsync($"Removed {channel?.Mention} from the ignore list.\n" +
                                                 $"Commands are now disabled in {channel?.Mention}",
                            Color.Green.RawValue);
                    else
                        await Context.ReplyAsync($"Removed {channel?.Mention} from the ignore list.\n" +
                                                 $"Commands are now enabled in {channel?.Mention}",
                            Color.Green.RawValue);
                }
            }
        }

        [Name("Ignore toggle")]
        [Command("ignore toggle")]
        [Description("Toggles whether common commands is only usable in ignored channels or not")]
        [Remarks("ignore toggle")]
        public async Task ToggleIgnoreChannelAsync()
        {
            using (var db = new DbService())
            {
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
        }

        [Name("Ignore list")]
        [Command("ignore list")]
        [Description("Toggles whether common commands is only usable in ignored channels or not")]
        [Remarks("ignore list")]
        public async Task ListIgnoreChannelsAsync()
        {
            using (var db = new DbService())
            {
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
                var embed = new EmbedBuilder().CreateDefault(content, Context.Guild.Id)
                    .WithAuthor(new EmbedAuthorBuilder {IconUrl = Context.Guild.IconUrl, Name = title});
                await Context.ReplyAsync(embed);
            }
        }
    }
}