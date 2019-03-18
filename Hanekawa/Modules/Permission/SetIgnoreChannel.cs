using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Preconditions;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Permission
{
    [Name("Channel ignore")]
    [Summary("Manages channels its to ignore or only use common commands on")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public class SetIgnoreChannel : InteractiveBase
    {
        private readonly RequiredChannel _requiredChannel;

        public SetIgnoreChannel(RequiredChannel requiredChannel) => _requiredChannel = requiredChannel;

        [Name("Channel ignore add")]
        [Command("channel ignore add", RunMode = RunMode.Async)]
        [Alias("ignore add")]
        [Summary("**Require Manage Server**\nAdds a channel to the command ignore list")]
        [Remarks("h.ignore add #general")]
        public async Task AddIgnoreChannel(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateAdminConfigAsync(Context.Guild);
                if (channel == null) channel = Context.Channel as ITextChannel;
                var result = await _requiredChannel.AddChannel(channel);
                if (!result)
                {
                    await Context.ReplyAsync(
                        $"Couldn't add {channel?.Mention} to the list. Its either already added or doesn't exist.",
                        Color.Red.RawValue);
                    return;
                }

                if (cfg.IgnoreAllChannels)
                    await Context.ReplyAsync($"Added {channel?.Mention} to the ignore list.\n" +
                                             $"Commands are now enabled in {channel?.Mention}", Color.Green.RawValue);
                else
                    await Context.ReplyAsync($"Added {channel?.Mention} to the ignore list.\n" +
                                             $"Commands are now disabled in {channel?.Mention}", Color.Green.RawValue);
            }
        }

        [Name("Channel ignore remove")]
        [Command("channel ignore remove", RunMode = RunMode.Async)]
        [Alias("ignore remove")]
        [Summary("**Require Manage Server**\nRemoves a channel to the command ignore list")]
        [Remarks("h.ignore remove #general")]
        public async Task RemoveIgnoreChannel(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateAdminConfigAsync(Context.Guild);
                if (channel == null) channel = Context.Channel as ITextChannel;
                var result = await _requiredChannel.RemoveChannel(channel);
                if (!result)
                {
                    await Context.ReplyAsync(
                        $"Couldn't remove {channel.Mention} from the list. Its either already not added or doesn't exist.",
                        Color.Red.RawValue);
                    return;
                }

                if (cfg.IgnoreAllChannels)
                    await Context.ReplyAsync($"Removed {channel?.Mention} from the ignore list.\n" +
                                             $"Commands are now disabled in {channel?.Mention}", Color.Green.RawValue);
                else
                    await Context.ReplyAsync($"Removed {channel?.Mention} from the ignore list.\n" +
                                             $"Commands are now enabled in {channel?.Mention}", Color.Green.RawValue);
            }
        }

        [Name("Channel ignore list")]
        [Command("channel ignore list", RunMode = RunMode.Async)]
        [Alias("ignore list")]
        [Summary("**Require Manage Server**\nList channels on the ignore list")]
        [Remarks("h.ignore list")]
        public async Task ListIgnoreChannel()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateAdminConfigAsync(Context.Guild);
                var list = await db.IgnoreChannels.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                string content = null;
                if (list.Count != 0)
                {
                    var channels = new List<string>();
                    foreach (var x in list)
                        try
                        {
                            var channel = Context.Guild.GetTextChannel(x.ChannelId);
                            if (channel == null) continue;
                            channels.Add(channel.Mention);
                        }
                        catch
                        {
                            /* ignored */
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

        [Name("Channel ignore toggle")]
        [Command("channel ignore toggle", RunMode = RunMode.Async)]
        [Alias("ignore toggle")]
        [Summary("**Require Manage Server**\nToggles whether the channels on the list are ignored, or only channels you can use commands on.")]
        [Remarks("h.ignore toggle")]
        public async Task ToggleIgnore()
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
    }
}