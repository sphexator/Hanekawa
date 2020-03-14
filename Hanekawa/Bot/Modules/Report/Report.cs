using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using Quartz.Util;

namespace Hanekawa.Bot.Modules.Report
{
    [Name("Report")]
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    public class Report : DiscordModuleBase<HanekawaContext>
    {
        private readonly ColourService _colour;

        public Report(ColourService colour) => _colour = colour;

        [Name("Report")]
        [Command("report")]
        [Description("Send a report to the moderator team")]
        public async Task ReportGuildAsync([Remainder] string text)
        {
            await Context.Message.TryDeleteMessageAsync();
            if (text.IsNullOrWhiteSpace()) return;
            using var db = new DbService();

            var report = await db.CreateReport(Context.User, Context.Guild, DateTime.UtcNow);
            var cfg = await db.GetOrCreateChannelConfigAsync(Context.Guild);
            if (!cfg.ReportChannel.HasValue) return;
            var embed = new EmbedBuilder().Create(text, Context.Colour.Get(Context.Guild.Id))
                .WithAuthor(new EmbedAuthorBuilder
                {
                    IconUrl = Context.User.GetAvatar(),
                    Name = Context.User.GetName()
                })
                .WithFooter(new EmbedFooterBuilder {Text = $"Report ID: {report.Id} - UserId: {Context.User.Id}"})
                .WithTimestamp(new DateTimeOffset(DateTime.UtcNow));

            if (Context.Message.Attachments.FirstOrDefault() != null)
                embed.ImageUrl = Context.Message.Attachments.First().Url;
            var msg = await Context.Guild.GetTextChannel(cfg.ReportChannel.Value).ReplyAsync(embed);
            report.MessageId = msg.Id;
            await db.SaveChangesAsync();
            await ReplyAndDeleteAsync(null, false,
                new EmbedBuilder().Create("Report sent!", Color.Green).Build());
        }

        [Name("Respond")]
        [Command("respond")]
        [Description("Respond to a report that's been sent")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RespondAsync(int id, [Remainder] string text)
        {
            if (text.IsNullOrWhiteSpace()) return;
            using var db = new DbService();

            var report = await db.Reports.FindAsync(id, Context.Guild.Id);
            var cfg = await db.GetOrCreateChannelConfigAsync(Context.Guild);

            if (report?.MessageId == null || !cfg.ReportChannel.HasValue) return;

            var msg = await Context.Guild.GetTextChannel(cfg.ReportChannel.Value)
                .GetMessageAsync(report.MessageId.Value);
            var embed = msg.Embeds.First().ToEmbedBuilder();
            embed.Color = Color.Orange;
            embed.AddField(Context.User.GetName(), text);
            try
            {
                var suggestUser = Context.Guild.GetUser(report.UserId);
                await (await suggestUser.GetOrCreateDMChannelAsync()).ReplyAsync(
                    "Your report got a response!\n" +
                    "report:\n" +
                    $"{embed.Description.Truncate(400)}\n" +
                    $"Answer from {Context.User.Mention}:\n" +
                    $"{text}", _colour.Get(Context.Guild.Id));
            }
            catch
            {
                /*IGNORE*/
            }

            await ((IUserMessage) msg).ModifyAsync(x => x.Embed = embed.Build());
        }

        [Name("Channel")]
        [Command("rc")]
        [Description("Sets a channel as channel to receive reports. don't mention a channel to disable reports.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetReportChannelAsync(SocketTextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateChannelConfigAsync(Context.Guild);
                if (cfg.ReportChannel.HasValue && channel == null)
                {
                    cfg.ReportChannel = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Disabled report channel", Color.Green);
                    return;
                }

                if (channel == null) channel = Context.Channel;
                cfg.ReportChannel = channel.Id;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"All reports will now be sent to {channel.Mention} !",
                    Color.Green);
            }
        }
    }
}