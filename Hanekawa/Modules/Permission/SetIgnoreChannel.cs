using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions;
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
        private readonly DbService _db;
        private readonly RequiredChannel _requiredChannel;

        public SetIgnoreChannel(RequiredChannel requiredChannel, DbService db)
        {
            _requiredChannel = requiredChannel;
            _db = db;
        }

        [Command("add", RunMode = RunMode.Async)]
        public async Task AddIgnoreChannel(ITextChannel channel = null)
        {
            var cfg = await _db.GetOrCreateGuildConfig(Context.Guild);
            if (channel == null) channel = Context.Channel as ITextChannel;
            var result = await _requiredChannel.AddChannel(channel);
            if (!result)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply(
                            $"Couldn't add {channel?.Mention} to the list. Its either already added or doesn't exist.",
                            Color.Red.RawValue).Build());
                return;
            }

            if (cfg.IgnoreAllChannels)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Added {channel?.Mention} to the ignore list.\n" +
                                             $"Commands are now enabled in {channel?.Mention}", Color.Green.RawValue)
                        .Build());
            }
            else
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Added {channel?.Mention} to the ignore list.\n" +
                                             $"Commands are now disabled in {channel?.Mention}", Color.Green.RawValue)
                        .Build());
            }
        }

        [Command("remove", RunMode = RunMode.Async)]
        public async Task RemoveIgnoreChannel(ITextChannel channel = null)
        {
            var cfg = await _db.GetOrCreateGuildConfig(Context.Guild);
            if (channel == null) channel = Context.Channel as ITextChannel;
            var result = await _requiredChannel.RemoveChannel(channel);
            if (!result)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply(
                            $"Couldn't remove {channel.Mention} from the list. Its either already not added or doesn't exist.",
                            Color.Red.RawValue).Build());
                return;
            }
            if (cfg.IgnoreAllChannels)
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Removed {channel?.Mention} to the ignore list.\n" +
                                             $"Commands are now disabled in {channel?.Mention}", Color.Green.RawValue)
                        .Build());
            }
            else
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Removed {channel?.Mention} to the ignore list.\n" +
                                             $"Commands are now enabled in {channel?.Mention}", Color.Green.RawValue)
                        .Build());
            }
            await ReplyAsync(null, false,
                new EmbedBuilder().Reply($"Removed {channel.Mention} from the ignore list.", Color.Green.RawValue)
                    .Build());
        }

        [Command("list", RunMode = RunMode.Async)]
        public async Task ListIgnoreChannel()
        {
            var cfg = await _db.GetOrCreateGuildConfig(Context.Guild);
            var list = await _db.IgnoreChannels.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
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
                
            var author = new EmbedAuthorBuilder
            {
                IconUrl = Context.Guild.IconUrl,
                Name = title
            };
            var embed = new EmbedBuilder
            {
                Color = Color.Purple,
                Description = content,
                Author = author
            };
            await ReplyAsync(null, false, embed.Build());
        }

        [Command("toggle", RunMode = RunMode.Async)]
        public async Task ToggleIgnore()
        {
            var cfg = await _db.GetOrCreateGuildConfig(Context.Guild);
            if (cfg.IgnoreAllChannels)
            {
                cfg.IgnoreAllChannels = false;
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply("Commands are now only usable in channels on the list.").Build());
            }
            else
            {
                cfg.IgnoreAllChannels = true;
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply("Commands are now usable in all channels beside those in the ignore list.")
                        .Build());
            }

            await _db.SaveChangesAsync();
        }
    }
}