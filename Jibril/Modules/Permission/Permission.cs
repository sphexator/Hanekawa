using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Extensions;
using Hanekawa.Preconditions;
using Hanekawa.Services;
using Hanekawa.Services.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Permission
{
    public class Permission : ModuleBase<SocketCommandContext>
    {
        [Command("permissions", RunMode = RunMode.Async)]
        [Alias("perm")]
        [Summary("Permission overview")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task ViewPermissionsAsync()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                var author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.Guild.IconUrl,
                    Name = $"Permissions for {Context.Guild.Name}"
                };
                var embed = new EmbedBuilder
                {
                    Color = Color.Purple,
                    Author = author
                };

                // Prefix
                embed.AddField("Prefix", cfg.Prefix ?? ".h", true);

                embed.AddField("Welcome Channel",
                    cfg.WelcomeChannel.HasValue
                        ? Context.Guild.GetTextChannel(cfg.WelcomeChannel.Value).Mention
                        : "Disabled",
                    true);

                // Level
                embed.AddField("Exp Multiplier", cfg.ExpMultiplier, true);
                embed.AddField("Level Role Stack", cfg.StackLvlRoles.ToString(), true);

                // Logging
                embed.AddField("Join/Leave Log",
                    cfg.LogJoin.HasValue ? Context.Guild.GetTextChannel(cfg.LogJoin.Value).Mention : "Disabled", true);
                embed.AddField("Avatar Log",
                    cfg.LogAvi.HasValue ? Context.Guild.GetTextChannel(cfg.LogAvi.Value).Mention : "Disabled", true);
                embed.AddField("Ban Log",
                    cfg.LogBan.HasValue ? Context.Guild.GetTextChannel(cfg.LogBan.Value).Mention : "Disabled", true);
                embed.AddField("Message Log",
                    cfg.LogMsg.HasValue ? Context.Guild.GetTextChannel(cfg.LogMsg.Value).Mention : "Disabled", true);

                // Other channel setup
                embed.AddField("Board Channel",
                    cfg.BoardChannel.HasValue
                        ? Context.Guild.GetTextChannel(cfg.BoardChannel.Value).Mention
                        : "Disabled", true);
                embed.AddField("Suggestion Channel",
                    cfg.SuggestionChannel.HasValue
                        ? Context.Guild.GetTextChannel(cfg.SuggestionChannel.Value).Mention
                        : "Disabled", true);
                embed.AddField("Report Channel",
                    cfg.ReportChannel.HasValue
                        ? Context.Guild.GetTextChannel(cfg.ReportChannel.Value).Mention
                        : "Disabled",
                    true);
                embed.AddField("Event Channel",
                    cfg.EventChannel.HasValue
                        ? Context.Guild.GetTextChannel(cfg.EventChannel.Value).Mention
                        : "Disabled", true);
                embed.AddField("Staff/Mod Channel",
                    cfg.ModChannel.HasValue ? Context.Guild.GetTextChannel(cfg.ModChannel.Value).Mention : "Disabled",
                    true);
                embed.AddField("Music Text Channel",
                    cfg.MusicChannel.HasValue
                        ? Context.Guild.GetTextChannel(cfg.MusicChannel.Value).Mention
                        : "Disabled",
                    true);
                embed.AddField("Music Voice Channel",
                    cfg.MusicVcChannel.HasValue
                        ? Context.Guild.GetVoiceChannel(cfg.MusicVcChannel.Value).Name
                        : "Disabled",
                    true);

                // Welcome
                embed.AddField("Welcome Banner", cfg.WelcomeBanner.ToString(), true);
                embed.AddField("Welcome Limit", $"{cfg.WelcomeLimit}", true);
                embed.AddField("Welcome Message", cfg.WelcomeMessage ?? "No message set", true);

                // Moderation
                string nudeChannels = null;
                foreach (var x in await db.NudeServiceChannels.Where(x => x.GuildId == Context.Guild.Id).ToListAsync())
                    nudeChannels += $"{Context.Guild.GetTextChannel(x.ChannelId).Mention} ({x.Tolerance}), ";
                embed.AddField("Invite Filter", cfg.FilterInvites.ToString(), true);
                embed.AddField("Toxicity Filter", nudeChannels ?? "Disabled");

                await ReplyAsync(null, false, embed.Build());
            }
        }

        [Group("set")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        public class SetPermission : ModuleBase<SocketCommandContext>
        {
            private readonly CommandHandlingService _command;

            public SetPermission(CommandHandlingService command)
            {
                _command = command;
            }

            [Command("prefix", RunMode = RunMode.Async)]
            public async Task SetPrefix(string prefix)
            {
                try
                {
                    await _command.UpdatePrefixAsync(Context.Guild, prefix);
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Successfully changed prefix to {prefix}!", Color.Green.RawValue)
                            .Build());
                }
                catch
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Something went wrong changing prefix to {prefix}...",
                            Color.Red.RawValue).Build());
                }
            }

            [Group("currency")]
            public class CurrencyOptions : InteractiveBase
            {
                [Command("name", RunMode = RunMode.Async)]
                public async Task SetCurrencyNameAsync([Remainder]string name = null)
                {
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                        if (name == null)
                        {
                            cfg.CurrencyName = "Special Currency";
                            await db.SaveChangesAsync();
                            await ReplyAsync(null, false,
                                new EmbedBuilder().Reply("Changed currency name back to default (Special Currency)",
                                    Color.Green.RawValue).Build());
                            return;
                        }

                        cfg.CurrencyName = name;
                        await db.SaveChangesAsync();
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply($"Changed currency name to `{name}` !",
                                Color.Green.RawValue).Build());
                    }
                }

                [Command("emote", RunMode = RunMode.Async)]
                public async Task SetCurrencyEmoteAsync(Emote emote = null)
                {
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                        if (emote == null)
                        {
                            cfg.EmoteCurrency = false;
                            cfg.CurrencySign = "$";
                            await db.SaveChangesAsync();
                            await ReplyAsync(null, false,
                                new EmbedBuilder().Reply("Changed currency sign back to default ($)",
                                    Color.Green.RawValue).Build());
                            return;
                        }

                        cfg.EmoteCurrency = true;
                        cfg.CurrencySign = $"<:{emote.Name}:{emote.Id}>";
                        await db.SaveChangesAsync();
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply($"Changed currency sign to {emote}",
                                Color.Green.RawValue).Build());
                    }
                }
            }
        }

        [Group("automod")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public class FilterPermission : InteractiveBase
        {
            [Command("invite")]
            [Alias("srvfilter")]
            [Summary("Toggles guild invite filter, auto-deletes invites")]
            public async Task InviteFilter()
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                    if (cfg.FilterInvites)
                    {
                        cfg.FilterInvites = false;
                        await db.SaveChangesAsync();
                        await ReplyAsync(null, false,
                            new EmbedBuilder()
                                .Reply("Disabled auto deletion and muting users posting invite links.",
                                    Color.Green.RawValue).Build());
                        return;
                    }

                    cfg.FilterInvites = true;
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Enabled auto deletion and muting users posting invite links.",
                            Color.Green.RawValue).Build());
                }
            }
        }

        [Group("log")]
        [Summary("Manage logging settings")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public class LogPermission : InteractiveBase
        {
            [Command("warn", RunMode = RunMode.Async)]
            [Summary("Enable/disable warn logging")]
            public async Task LogWarnAsync(ITextChannel channel = null)
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                    if (channel == null)
                    {
                        cfg.LogWarn = null;
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply("Disabled logging of warnings!", Color.Green.RawValue).Build());
                        await db.SaveChangesAsync();
                        return;
                    }

                    cfg.LogWarn = channel.Id;
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Set warn logging channel to {channel.Mention}!", Color.Green.RawValue).Build());
                    await db.SaveChangesAsync();
                }
            }

            [Command("join", RunMode = RunMode.Async)]
            [Summary("Enable/disable join/leaves logging")]
            public async Task LogJoinAsync(ITextChannel channel = null)
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                    if (channel == null)
                    {
                        cfg.LogJoin = null;
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply("Disabled logging of join/leave!", Color.Green.RawValue).Build());
                        await db.SaveChangesAsync();
                        return;
                    }

                    cfg.LogJoin = channel.Id;
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Set join/leave logging channel to {channel.Mention}!",
                            Color.Green.RawValue).Build());
                    await db.SaveChangesAsync();
                }
            }

            [Command("message", RunMode = RunMode.Async)]
            [Alias("msg")]
            [Summary("Enable/Disable message logging")]
            public async Task LogMessageAsync(ITextChannel channel = null)
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                    if (channel == null)
                    {
                        cfg.LogMsg = null;
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply("Disabled logging of messages!", Color.Green.RawValue).Build());
                        await db.SaveChangesAsync();
                        return;
                    }

                    cfg.LogMsg = channel.Id;
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Set message logging channel to {channel.Mention}!",
                            Color.Green.RawValue).Build());
                    await db.SaveChangesAsync();
                }
            }

            [Command("ban", RunMode = RunMode.Async)]
            [Alias("ban")]
            [Summary("Enable/Disable moderation logging")]
            public async Task LogBanAsync(ITextChannel channel = null)
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                    if (channel == null)
                    {
                        cfg.LogBan = null;
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply("Disabled logging of moderation actions!", Color.Green.RawValue)
                                .Build());
                        await db.SaveChangesAsync();
                        return;
                    }

                    cfg.LogBan = channel.Id;
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Set mod log channel to {channel.Mention}!", Color.Green.RawValue)
                            .Build());
                    await db.SaveChangesAsync();
                }
            }
        }

        [Group("channel")]
        [Alias("ignore")]
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

                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Added {channel?.Mention} to the ignore list.", Color.Green.RawValue)
                        .Build());
            }

            [Command("remove", RunMode = RunMode.Async)]
            public async Task RemoveIgnoreChannel(ITextChannel channel = null)
            {
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

                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Removed {channel.Mention} from the ignore list.", Color.Green.RawValue)
                        .Build());
            }

            [Command("list", RunMode = RunMode.Async)]
            public async Task ListIgnoreChannel()
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
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

                    string title = null;
                    title = cfg.IgnoreAllChannels
                        ? $"Channels commands are enabled on:"
                        : $"Channels commands are ignored on:";
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
            }

            [Command("toggle", RunMode = RunMode.Async)]
            public async Task ToggleIgnore()
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                    if (cfg.IgnoreAllChannels)
                    {
                        cfg.IgnoreAllChannels = false;
                        await ReplyAsync(null, false,
                            new EmbedBuilder()
                                .Reply("Commands are now usable in all channels beside those in the ignore list.")
                                .Build());
                    }
                    else
                    {
                        cfg.IgnoreAllChannels = true;
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply("Commands are now only usable in channels on the list.").Build());
                    }

                    await db.SaveChangesAsync();
                }
            }
        }
    }
}