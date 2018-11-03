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

namespace Hanekawa.Modules.QA
{
    public class QuestionAndAnswer : InteractiveBase
    {
        [Command("question", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [Summary("Sends a question to server owners to answer (QnA)")]
        public async Task QuestionAsync([Remainder] string question)
        {
            await Context.Message.DeleteAsync();
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (!cfg.QuestionAndAnswerChannel.HasValue) return;
                var caseId = await db.CreateQnA(Context.User, Context.Guild, DateTime.UtcNow);
                var author = new EmbedAuthorBuilder
                {
                    IconUrl = (Context.User as SocketGuildUser).GetAvatar(),
                    Name = (Context.User as SocketGuildUser).GetName()
                };
                var footer = new EmbedFooterBuilder
                {
                    Text = $"Question ID: {caseId.Id}"
                };
                var embed = new EmbedBuilder
                {
                    Author = author,
                    Timestamp = DateTimeOffset.UtcNow,
                    Color = Color.Purple,
                    Description = question,
                    Footer = footer
                };
                var msg = await Context.Guild.GetTextChannel(cfg.QuestionAndAnswerChannel.Value).SendEmbedAsync(embed);
                caseId.MessageId = msg.Id;
                await db.SaveChangesAsync();
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().Reply("Question sent!", Color.Green.RawValue).Build());
            }
        }

        [Command("qa answer", RunMode = RunMode.Async)]
        [Alias("qaa")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Users with manage guild perms can answer questions sent in.")]
        public async Task CommentAsync(uint id, [Remainder] string response)
        {
            await Context.Message.DeleteAsync();
            using (var db = new DbService())
            {
                var question = await db.QuestionAndAnswers.FindAsync(id, Context.Guild.Id);
                if (question == null) return;
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (!cfg.QuestionAndAnswerChannel.HasValue)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("No QnA channel has been setup", Color.Red.RawValue).Build());
                    return;
                }

                var msg = await Context.Guild.GetTextChannel(cfg.QuestionAndAnswerChannel.Value)
                    .GetMessageAsync(question.MessageId.Value);
                var embed = msg.Embeds.First().ToEmbedBuilder();
                var field = new EmbedFieldBuilder
                {
                    Name = (Context.User as SocketGuildUser).GetName(),
                    Value = response
                };
                embed.AddField(field);
                try
                {
                    var suggestUser = Context.Guild.GetUser(question.UserId);
                    await (await suggestUser.GetOrCreateDMChannelAsync()).SendMessageAsync(null, false,
                        new EmbedBuilder().Reply(
                            "Your question got a response!\n" +
                            "Question:\n" +
                            $"{embed.Description}\n" +
                            $"Answer from {Context.User}:\n" +
                            $"{response}").Build());
                }
                catch
                {
                    /*IGNORE*/
                }

                await (msg as IUserMessage).ModifyAsync(x => x.Embed = embed.Build());
            }
        }

        [Command("qa set channel", RunMode = RunMode.Async)]
        [Alias("qac", "qachannel", "qa channel")]
        [Summary("Sets a channel for QnA. Leave empty channel to disable QnA.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task SetQuestionChannelAsync(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (cfg.QuestionAndAnswerChannel.HasValue && channel == null)
                {
                    cfg.QuestionAndAnswerChannel = null;
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Disabled QnA channel", Color.Green.RawValue).Build());
                    return;
                }

                if (channel == null) channel = Context.Channel as ITextChannel;

                cfg.QuestionAndAnswerChannel = channel?.Id;
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"All questions will now be sent to {channel.Mention} !",
                        Color.Green.RawValue).Build());
            }
        }
    }
}