using System;
using System.Linq;
using Discord.Addons.Interactive;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Jibril.Extensions;
using Jibril.Services.Entities;

namespace Jibril.Modules.Report
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
                var report = await db.CreateReport(Context.User, DateTime.UtcNow);
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
                    Color = Color.Purple,
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

        [Command("report", RunMode = RunMode.Async)]
        [RequireContext(ContextType.DM)]
        public async Task ReportDmAsync([Remainder]string text)
        {
            using (var db = new DbService())
            {
                var report = await db.CreateReport(Context.User, DateTime.UtcNow);
                var guild = Context.Client.Guilds.FirstOrDefault(x => x.Id == 339370914724446208);
                var cfg = await db.GetOrCreateGuildConfig(guild);
                if (!cfg.ReportChannel.HasValue) return;
                var author = new EmbedAuthorBuilder
                {
                    IconUrl = (Context.User as SocketGuildUser).GetAvatar(),
                    Name = Context.User.Username
                };
                var footer = new EmbedFooterBuilder
                {
                    Text = $"{report.Id}"
                };
                var embed = new EmbedBuilder
                {
                    Author = author,
                    Footer = footer,
                    Color = Color.Purple,
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
                var report = await db.Reports.FindAsync(id);
                if (report == null) return;
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                var msg = await Context.Guild.GetTextChannel(cfg.ReportChannel.Value)
                    .GetMessageAsync(report.MessageId.Value);
                var embed = msg.Embeds.First().ToEmbedBuilder();
                embed.Color = Color.Orange;
                var field = new EmbedFieldBuilder
                {
                    Name = (Context.User as SocketGuildUser).GetName(),
                    Value = text
                };
                embed.AddField(field);
                try
                {
                    var suggestUser = Context.Guild.GetUser(report.UserId);
                    await (await suggestUser.GetOrCreateDMChannelAsync()).SendMessageAsync(
                        $"Your report got a response!\n" +
                        $"report:\n" +
                        $"{embed.Description}\n" +
                        $"Answer from {Context.User}:\n" +
                        $"{text}");
                }
                catch{ /*IGNORE*/ }

                await (msg as IUserMessage).ModifyAsync(x => x.Embed = embed.Build());
            }
        }
    }
}
