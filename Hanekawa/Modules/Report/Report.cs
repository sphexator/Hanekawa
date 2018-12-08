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
using Hanekawa.Extensions.Embed;
using Hanekawa.Preconditions;
using Humanizer;

namespace Hanekawa.Modules.Report
{
    public class Report : InteractiveBase
    {
        private readonly DbService _db;

        public Report(DbService db)
        {
            _db = db;
        }

        [Command("report channel", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        [Summary("Sets a channel as channel to recieve reports. don't mention a channel to disable reports.")]
        public async Task SetReportChannelAsync(ITextChannel channel = null)
        {
            var cfg = await _db.GetOrCreateGuildConfig(Context.Guild);
            if (cfg.ReportChannel.HasValue && channel == null)
            {
                cfg.ReportChannel = null;
                await _db.SaveChangesAsync();
                await Context.ReplyAsync("Disabled report channel", Color.Green.RawValue);
                return;
            }

            if (channel == null) channel = Context.Channel as ITextChannel;
            cfg.ReportChannel = channel?.Id;
            await _db.SaveChangesAsync();
            await Context.ReplyAsync($"All reports will now be sent to {channel.Mention} !",
                Color.Green.RawValue);
        }

        [Command("report", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [Ratelimit(1, 30, Measure.Seconds)]
        public async Task ReportGuildAsync([Remainder] string text)
        {
            await Context.Message.DeleteAsync();
            var report = await _db.CreateReport(Context.User, Context.Guild, DateTime.UtcNow);
            var cfg = await _db.GetOrCreateGuildConfig(Context.Guild);
            if (!cfg.ReportChannel.HasValue) return;
            var embed = new EmbedBuilder().CreateDefault(text)
                .WithAuthor(new EmbedAuthorBuilder
                {
                    IconUrl = (Context.User as SocketGuildUser).GetAvatar(),
                    Name = (Context.User as SocketGuildUser).GetName()
                })
                .WithFooter(new EmbedFooterBuilder {Text = $"Report ID: {report.Id} - UserId: {Context.User.Id}"})
                .WithTimestamp(new DateTimeOffset(DateTime.UtcNow));

            if (Context.Message.Attachments.FirstOrDefault() != null)
                embed.ImageUrl = Context.Message.Attachments.First().Url;
            var msg = await Context.Guild.GetTextChannel(cfg.ReportChannel.Value).ReplyAsync(embed);
            report.MessageId = msg.Id;
            await _db.SaveChangesAsync();
            await ReplyAndDeleteAsync(null, false,
                new EmbedBuilder().CreateDefault("Report sent!", Color.Green.RawValue).Build());
        }

        [Command("respond", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RespondAsync(uint id, [Remainder] string text)
        {
            var report = await _db.Reports.FindAsync(id, Context.Guild.Id);
            var cfg = await _db.GetOrCreateGuildConfig(Context.Guild);

            if (report?.MessageId == null || !cfg.ReportChannel.HasValue) return;

            var msg = await Context.Guild.GetTextChannel(cfg.ReportChannel.Value)
                .GetMessageAsync(report.MessageId.Value);
            var embed = msg.Embeds.First().ToEmbedBuilder();
            embed.Color = Color.Orange;
            embed.AddField((Context.User as SocketGuildUser).GetName(), text);
            try
            {
                var suggestUser = Context.Guild.GetUser(report.UserId);
                await (await suggestUser.GetOrCreateDMChannelAsync()).ReplyAsync(
                    "Your report got a response!\n" +
                    "report:\n" +
                    $"{embed.Description.Truncate(400)}\n" +
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