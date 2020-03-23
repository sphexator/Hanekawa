﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Humanizer;
using Qmmands;
using Quartz.Util;

namespace Hanekawa.Bot.Modules.Report
{
    [Name("Report")]
    [RequireBotGuildPermissions(Permission.EmbedLinks)]
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
            var embed = new LocalEmbedBuilder().Create(text, Context.Colour.Get(Context.Guild.Id))
                .WithAuthor(new LocalEmbedAuthorBuilder()
                {
                    IconUrl = Context.User.GetAvatarUrl(),
                    Name = Context.Member.DisplayName
                })
                .WithFooter(new LocalEmbedFooterBuilder {Text = $"Report ID: {report.Id} - UserId: {Context.User.Id}"})
                .WithTimestamp(new DateTimeOffset(DateTime.UtcNow));

            if (Context.Message.Attachments.FirstOrDefault() != null)
                embed.ImageUrl = Context.Message.Attachments.First().Url;
            var msg = await Context.Guild.GetTextChannel(cfg.ReportChannel.Value).ReplyAsync(embed);
            report.MessageId = msg.Id;
            await db.SaveChangesAsync();
            await Context.ReplyAndDeleteAsync(null, false,
                new LocalEmbedBuilder().Create("Report sent!", Color.Green));
        }

        [Name("Respond")]
        [Command("respond")]
        [Description("Respond to a report that's been sent")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task RespondAsync(int id, [Remainder] string text)
        {
            if (text.IsNullOrWhiteSpace()) return;
            using var db = new DbService();

            var report = await db.Reports.FindAsync(id, Context.Guild.Id);
            var cfg = await db.GetOrCreateChannelConfigAsync(Context.Guild);

            if (report?.MessageId == null || !cfg.ReportChannel.HasValue) return;

            var msg = Context.Guild.GetTextChannel(cfg.ReportChannel.Value)
                .GetMessage(report.MessageId.Value);
            var embed = msg.Embeds.First().ToEmbedBuilder();
            embed.Color = Color.Orange;
            embed.AddField(Context.Member.DisplayName, text);
            try
            {
                var suggestUser = Context.Guild.GetMember(report.UserId);
                if(suggestUser.DmChannel != null) await suggestUser.DmChannel.ReplyAsync(
                    "Your report got a response!\n" +
                    "report:\n" +
                    $"{embed.Description.Truncate(400)}\n" +
                    $"Answer from {Context.User.Mention}:\n" +
                    $"{text}", _colour.Get(Context.Guild.Id));
                else
                {
                    var dm = await suggestUser.CreateDmChannelAsync();
                    await dm.ReplyAsync("Your report got a response!\n" +
                                        "report:\n" +
                                        $"{embed.Description.Truncate(400)}\n" +
                                        $"Answer from {Context.User.Mention}:\n" +
                                        $"{text}", _colour.Get(Context.Guild.Id));
                }
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
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetReportChannelAsync(CachedTextChannel channel = null)
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