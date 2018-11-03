using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Preconditions;

namespace Hanekawa.Modules.Report
{
    public class Report : InteractiveBase
    {
        [Command("report channel", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        [Summary("Sets a channel as channel to recieve reports. don't mention a channel to disable reports.")]
        public async Task SetReportChannelAsync(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (cfg.ReportChannel.HasValue && channel == null)
                {
                    cfg.ReportChannel = null;
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Disabled report channel", Color.Green.RawValue).Build());
                    return;
                }

                cfg.ReportChannel = channel.Id;
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"All reports will now be sent to {channel.Mention} !",
                        Color.Green.RawValue).Build());
            }
        }

        [Command("report", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [Ratelimit(1, 30, Measure.Seconds)]
        public async Task ReportGuildAsync([Remainder] string text)
        {
            await Context.Message.DeleteAsync();
            using (var db = new DbService())
            {
                var report = await db.CreateReport(Context.User, Context.Guild, DateTime.UtcNow);
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (!cfg.ReportChannel.HasValue) return;
                var author = new EmbedAuthorBuilder
                {
                    IconUrl = (Context.User as SocketGuildUser).GetAvatar(),
                    Name = (Context.User as SocketGuildUser).GetName()
                };
                var footer = new EmbedFooterBuilder
                {
                    Text = $"Report ID: {report.Id} - UserId: {Context.User.Id}"
                };
                var embed = new EmbedBuilder
                {
                    Author = author,
                    Footer = footer,
                    Color = Color.Purple,
                    Description = text,
                    Timestamp = new DateTimeOffset(DateTime.UtcNow)
                };
                if (Context.Message.Attachments.FirstOrDefault() != null)
                    embed.ImageUrl = Context.Message.Attachments.First().Url;
                var msg = await Context.Guild.GetTextChannel(cfg.ReportChannel.Value).SendEmbedAsync(embed);
                report.MessageId = msg.Id;
                await db.SaveChangesAsync();
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().Reply("Report sent!", Color.Green.RawValue).Build());
            }
        }

        [Command("respond", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RespondAsync(uint id, [Remainder] string text)
        {
            using (var db = new DbService())
            {
                var report = await db.Reports.FindAsync(id, Context.Guild.Id);
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (report?.MessageId == null || !cfg.ReportChannel.HasValue) return;
                var msg = await Context.Guild.GetTextChannel(cfg.ReportChannel.Value)
                    .GetMessageAsync(report.MessageId.Value);
                var embed = msg.Embeds.First().ToEmbedBuilder();
                embed.Color = Color.Orange;
                embed.AddField((Context.User as SocketGuildUser).GetName(), text);
                try
                {
                    var suggestUser = Context.Guild.GetUser(report.UserId);
                    await (await suggestUser.GetOrCreateDMChannelAsync()).SendMessageAsync(
                        "Your report got a response!\n" +
                        "report:\n" +
                        $"{embed.Description}\n" +
                        $"Answer from {Context.User.Mention}:\n" +
                        $"{text}");
                }
                catch
                {
                    /*IGNORE*/
                }

                await ((IUserMessage) msg).ModifyAsync(x => x.Embed = embed.Build());
            }
        }
    }
}