using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Jibril.Extensions;
using Jibril.Services;
using Jibril.Services.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jibril.Modules.Administration
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
                embed.AddField("Prefix", cfg.Prefix, true);

                if (cfg.WelcomeChannel.HasValue)
                    embed.AddField($"Welcome Channel", Context.Guild.GetTextChannel(cfg.WelcomeChannel.Value).Mention,
                        true);
                else embed.AddField($"Welcome Log", "Disabled", true);

                embed.AddField($"Welcome Banner", cfg.WelcomeBanner.ToString(), true);
                embed.AddField($"Welcome Limit", $"{cfg.WelcomeLimit}", true);
                embed.AddField($"Welcome Message", cfg.WelcomeMessage, true);

                // Level
                embed.AddField("Exp Multiplier", cfg.ExpMultiplier, true);
                embed.AddField("Level Role Stack", cfg.StackLvlRoles.ToString(), true);

                // Logging
                embed.AddField($"Join/Leave Log",
                    cfg.LogJoin.HasValue ? Context.Guild.GetTextChannel(cfg.LogJoin.Value).Mention : "Disabled", true);
                embed.AddField($"Avatar Log",
                    cfg.LogAvi.HasValue ? Context.Guild.GetTextChannel(cfg.LogAvi.Value).Mention : "Disabled", true);
                embed.AddField($"Ban Log",
                    cfg.LogBan.HasValue ? Context.Guild.GetTextChannel(cfg.LogBan.Value).Mention : "Disabled", true);
                embed.AddField($"Message Log",
                    cfg.LogMsg.HasValue ? Context.Guild.GetTextChannel(cfg.LogMsg.Value).Mention : "Disabled", true);

                // Moderation
                string nudeChannels = null;
                foreach (var x in await db.NudeServiceChannels.Where(x => x.GuildId == Context.Guild.Id).ToListAsync())
                    nudeChannels += $"{Context.Guild.GetTextChannel(x.ChannelId).Mention} ({x.Tolerance}), ";
                embed.AddField("Invite Filter", cfg.FilterInvites.ToString(), true);
                embed.AddField("Toxicity Filter", nudeChannels ?? "Disabled");

                // Other channel setup
                if (cfg.BoardChannel.HasValue)
                    embed.AddField("Board Channel", Context.Guild.GetTextChannel(cfg.BoardChannel.Value).Mention, true);
                else embed.AddField($"Board Channel", "Disabled", true);

                if (cfg.SuggestionChannel.HasValue)
                    embed.AddField("Suggestion Channel",
                        Context.Guild.GetTextChannel(cfg.SuggestionChannel.Value).Mention, true);
                else embed.AddField($"Suggestion Channel", "Disabled", true);

                if (cfg.ReportChannel.HasValue)
                    embed.AddField("Report Channel", Context.Guild.GetTextChannel(cfg.ReportChannel.Value).Mention,
                        true);
                else embed.AddField($"Report Channel", "Disabled", true);

                if (cfg.EventChannel.HasValue)
                    embed.AddField("Event Channel", Context.Guild.GetTextChannel(cfg.EventChannel.Value).Mention, true);
                else embed.AddField($"Event Channel", "Disabled", true);

                if (cfg.ModChannel.HasValue)
                    embed.AddField("Staff/Mod Channel", Context.Guild.GetTextChannel(cfg.ModChannel.Value).Mention,
                        true);
                else embed.AddField($"Welcome Channel", "Disabled", true);

                if (cfg.MusicChannel.HasValue)
                    embed.AddField("Music Text Channel", Context.Guild.GetTextChannel(cfg.MusicChannel.Value).Mention,
                        true);
                else embed.AddField($"Music Text Channel", "Disabled", true);

                if (cfg.MusicVcChannel.HasValue)
                    embed.AddField("Music Voice Channel", Context.Guild.GetVoiceChannel(cfg.MusicVcChannel.Value).Name,
                        true);
                else embed.AddField($"Music Voice Channel", "Disabled", true);
            }
        }

        [Group("set")]
        [RequireUserPermission(GuildPermission.Administrator)]
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
    }
}