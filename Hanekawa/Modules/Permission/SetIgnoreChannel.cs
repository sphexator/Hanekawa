using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Preconditions;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Permission
{
    [Name("Channel ignore")]
    [Group("channel ignore")]
    [Alias("ignore", "chignore", "chi")]
    [Summary("Manages channels its to ignore or only use common commands on")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public class SetIgnoreChannel : InteractiveBase
    {
        private readonly RequiredChannel _requiredChannel;

        public SetIgnoreChannel(RequiredChannel requiredChannel)
        {
            _requiredChannel = requiredChannel;
        }

        [Command("add", RunMode = RunMode.Async)]
        public async Task AddIgnoreChannel(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                if (channel == null) channel = Context.Channel as ITextChannel;
                var result = await _requiredChannel.AddChannel(channel);
                if (!result)
                {
                    await Context.ReplyAsync($"Couldn't add {channel?.Mention} to the list. Its either already added or doesn't exist.",
                        Color.Red.RawValue);
                    return;
                }

                if (cfg.IgnoreAllChannels)
                {
                    await Context.ReplyAsync($"Added {channel?.Mention} to the ignore list.\n" +
                                             $"Commands are now enabled in {channel?.Mention}", Color.Green.RawValue);
                }
                else
                {
                    await Context.ReplyAsync($"Added {channel?.Mention} to the ignore list.\n" +
                                             $"Commands are now disabled in {channel?.Mention}", Color.Green.RawValue);
                }
            }
        }

        [Command("remove", RunMode = RunMode.Async)]
        public async Task RemoveIgnoreChannel(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                if (channel == null) channel = Context.Channel as ITextChannel;
                var result = await _requiredChannel.RemoveChannel(channel);
                if (!result)
                {
                    await Context.ReplyAsync($"Couldn't remove {channel.Mention} from the list. Its either already not added or doesn't exist.",
                        Color.Red.RawValue);
                    return;
                }
                if (cfg.IgnoreAllChannels)
                {
                    await Context.ReplyAsync($"Removed {channel?.Mention} from the ignore list.\n" +
                                             $"Commands are now disabled in {channel?.Mention}", Color.Green.RawValue);
                }
                else
                {
                    await Context.ReplyAsync($"Removed {channel?.Mention} from the ignore list.\n" +
                                             $"Commands are now enabled in {channel?.Mention}", Color.Green.RawValue);
                }
            }
        }

        [Command("list", RunMode = RunMode.Async)]
        public async Task ListIgnoreChannel()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                var list = await db.IgnoreChannels.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                string content = null;
                if (list.Count != 0)
                {
                    var channels = new List<string>();
                    foreach (var x in list)
                        try
                        {
                            channels.Add(Context.Guild.GetTextChannel(x.ChannelId).Mention);
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
                    .WithAuthor(new EmbedAuthorBuilder { IconUrl = Context.Guild.IconUrl, Name = title });
                await Context.ReplyAsync(embed);
            }
        }

        [Command("toggle", RunMode = RunMode.Async)]
        public async Task ToggleIgnore()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                if (cfg.IgnoreAllChannels)
                {
                    cfg.IgnoreAllChannels = false;
                    await Context.ReplyAsync("Commands are now only usable in channels on the list.");
                }
                else
                {
                    cfg.IgnoreAllChannels = true;
                    await Context.ReplyAsync("Commands are now usable in all channels beside those in the ignore list.");
                }

                await db.SaveChangesAsync();
            }
        }
    }
}