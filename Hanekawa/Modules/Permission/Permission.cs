using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Services.CommandHandler;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Permission
{
    [RequireContext(ContextType.Guild)]
    public class Permission : InteractiveBase
    {
        private readonly CommandHandlingService _command;

        public Permission(CommandHandlingService command)
        {
            _command = command;
        }

        [Command("permissions", RunMode = RunMode.Async)]
        [Alias("perm")]
        [Summary("Permission overview")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
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

        [Command("set prefix", RunMode = RunMode.Async)]
        [Summary("Sets custom prefix for this guild/server")]
        [RequireUserPermission(GuildPermission.Administrator)]
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
}