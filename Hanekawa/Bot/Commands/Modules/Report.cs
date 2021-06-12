using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Entities.Color;
using Hanekawa.Extensions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using Quartz.Util;

namespace Hanekawa.Bot.Commands.Modules
{
    [Name("Report")]
    [Description("Commands to report a issue to the moderation team")]
    public class Report : HanekawaCommandModule
    {
        [Name("Report")]
        [Command("report")]
        [Description("Send a report to the moderator team")]
        public async Task ReportGuildAsync([Remainder] string text)
        {
            await Context.Message.TryDeleteMessageAsync();
            if (text.IsNullOrWhiteSpace()) return;

            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();

            var report = await db.CreateReport(Context.Author, Context.Guild, DateTime.UtcNow);
            var cfg = await db.GetOrCreateChannelConfigAsync(Context.Guild);
            if (!cfg.ReportChannel.HasValue) return;

            var builder = new LocalMessage
            {
                Embed = new LocalEmbed
                {
                    Author = new LocalEmbedAuthor()
                    {
                        IconUrl = Context.Author.GetAvatarUrl(),
                        Name = Context.Author.DisplayName()
                    },
                    Color = Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId),
                    Description = text,
                    Footer = new LocalEmbedFooter
                    {
                        Text = $"Report ID: {report.Id} - UserId: {Context.Author.Id}"
                    },
                    Timestamp = DateTimeOffset.UtcNow
                }
            };
            var msg = await (Context.Guild.GetChannel(cfg.ReportChannel.Value) as ITextChannel).SendMessageAsync(builder);
            report.MessageId = msg.Id;
            await db.SaveChangesAsync();
            await ReplyAndDeleteAsync(new LocalMessage().Create("Report sent!", HanaBaseColor.Ok()));
        }

        [Name("Respond")]
        [Command("respond")]
        [Description("Respond to a report that's been sent")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task RespondAsync(int id, [Remainder] string text)
        {
            if (text.IsNullOrWhiteSpace()) return;

            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();

            var report = await db.Reports.FindAsync(id, Context.Guild.Id);
            var cfg = await db.GetOrCreateChannelConfigAsync(Context.Guild);

            if (report?.MessageId == null || !cfg.ReportChannel.HasValue) return;

            var msg =
                await (Context.Guild.GetChannel(cfg.ReportChannel.Value) as ITextChannel).GetOrFetchMessageAsync(
                    report.MessageId.Value);
            var embed = LocalEmbed.FromEmbed(msg.Embeds[0]);
            embed.Color = HanaBaseColor.Orange();
            embed.AddField(Context.Author.DisplayName(), text);
            try
            {
                var reporter = await Context.Guild.GetOrFetchMemberAsync(report.UserId);
                await reporter.SendMessageAsync(new LocalMessage
                {
                    Embed = new LocalEmbed
                    {
                        Description =
                            $"Your report got a response!\nReport:\n{embed.Description.Truncate(300)}\nAnswer from {Context.Author.Mention}:\n{text.Truncate(1500)}",
                        Color = Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId)
                    }
                });
            }
            catch
            {
                /*IGNORE*/
            }

            await msg.ModifyAsync(x => x.Embed = embed);
        }
        
        [Name("Report Admin")]
        [Description("Commands to configure the report module")]
        [Group("Report")]
        public class ReportAdmin : Report
        {
            [Name("Channel")]
            [Command("channel")]
            [Description("Sets a channel as channel to receive reports. don't mention a channel to disable reports.")]
            [RequireAuthorGuildPermissions(Permission.ManageGuild)]
            public async Task SetReportChannelAsync(ITextChannel channel = null)
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateChannelConfigAsync(Context.Guild);
                if (cfg.ReportChannel.HasValue && channel == null)
                {
                    cfg.ReportChannel = null;
                    await db.SaveChangesAsync();
                    await Reply("Disabled report channel", HanaBaseColor.Ok());
                    return;
                }

                channel ??= Context.Channel;
                if (channel == null) return;
                cfg.ReportChannel = channel.Id;
                await db.SaveChangesAsync();
                await Reply($"All reports will now be sent to {channel.Mention} !",
                    HanaBaseColor.Ok());
            }
        }
    }
}