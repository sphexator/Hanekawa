using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Services.Entities;

namespace Hanekawa.Modules.Report
{
    public class Report : InteractiveBase
    {
        [Command("report", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task ReportGuildAsync([Remainder]string text)
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
                    Text = $"{report.Id}"
                };
                var embed = new EmbedBuilder
                {
                    Author = author,
                    Footer = footer,
                    Color = Color.DarkPurple,
                    Description = text
                };
                if (Context.Message.Attachments.FirstOrDefault() != null)
                {
                    embed.ImageUrl = Context.Message.Attachments.First().Url;
                }
                await Context.Guild.GetTextChannel(cfg.ReportChannel.Value).SendEmbedAsync(embed);
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().Reply("Report sent!", Color.Green.RawValue).Build());
            }
        }

        [Command("respond", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
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
                catch{ /*IGNORE*/ }

                await ((IUserMessage) msg).ModifyAsync(x => x.Embed = embed.Build());
            }
        }
    }
}
