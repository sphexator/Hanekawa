using System;
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

namespace Hanekawa.Modules.Administration
{
    public class Permission : ModuleBase<SocketCommandContext>
    {
        [Command("permissions", RunMode = RunMode.Async)]
        [Alias("perm")]
        [RequireUserPermission(GuildPermission.Administrator)]
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

                embed.AddField("Welcome Banner", cfg.WelcomeBanner.ToString(), true);
                embed.AddField("Welcome Limit", $"{cfg.WelcomeLimit}", true);
                embed.AddField("Welcome Message", cfg.WelcomeMessage ?? "No message set", true);

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
        }

        [Group("log")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public class LogPermission : InteractiveBase
        {
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
                        await ReplyAsync(null, false, new EmbedBuilder().Reply("Disabled logging of join/leave!", Color.Green.RawValue).Build());
                        await db.SaveChangesAsync();
                        return;
                    }

                    cfg.LogJoin = channel.Id;
                    await ReplyAsync(null, false, new EmbedBuilder().Reply($"Set join/leave logging channel to {channel.Mention}!", Color.Green.RawValue).Build());
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
                        await ReplyAsync(null, false, new EmbedBuilder().Reply("Disabled logging of messages!", Color.Green.RawValue).Build());
                        await db.SaveChangesAsync();
                        return;
                    }

                    cfg.LogMsg = channel.Id;
                    await ReplyAsync(null, false, new EmbedBuilder().Reply($"Set message logging channel to {channel.Mention}!", Color.Green.RawValue).Build());
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
                        await ReplyAsync(null, false, new EmbedBuilder().Reply("Disabled logging of moderation actions!", Color.Green.RawValue).Build());
                        await db.SaveChangesAsync();
                        return;
                    }

                    cfg.LogBan = channel.Id;
                    await ReplyAsync(null, false, new EmbedBuilder().Reply($"Set mod log channel to {channel.Mention}!", Color.Green.RawValue).Build());
                    await db.SaveChangesAsync();
                }
            }
        }

        [Group("ignore")]
        [RequireUserPermission(GuildPermission.Administrator)]
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
                if(channel == null) channel = Context.Channel as ITextChannel;
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
                    var channels = new List<ITextChannel>();
                    foreach (var x in await db.IgnoreChannels.Where(x => x.GuildId == Context.Guild.Id).ToListAsync())
                    {
                        try { channels.Add(Context.Guild.GetTextChannel(x.ChannelId)); }
                        catch {  /* ignored */ }

                        var content = string.Join("\n", channels);
                        string title = null;
                        title = cfg.IgnoreAllChannels
                            ? $"Channels commands are enabled:"
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
                        await ReplyAsync(null, false, new EmbedBuilder().Reply("Commands are now usable in all channels beside those in the ignore list.").Build());
                    }
                    else
                    {
                        cfg.IgnoreAllChannels = true;
                        await ReplyAsync(null, false, new EmbedBuilder().Reply("Commands are now only usable in channels on the list.").Build());
                    }

                    await db.SaveChangesAsync();
                }
            }
        }
    }
}