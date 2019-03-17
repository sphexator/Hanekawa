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

namespace Hanekawa.Modules.QA
{
    [Name("Q&A")]
    [Summary("Question and answer or AMA")]
    public class QuestionAndAnswer : InteractiveBase
    {
        [Name("Question")]
        [Command("question", RunMode = RunMode.Async)]
        [Summary("Sends a question to server owners to answer (QnA)")]
        [Remarks("h.question Here is my question :pog:")]
        [RequireContext(ContextType.Guild)]
        public async Task QuestionAsync([Remainder] string question)
        {
            using (var db = new DbService())
            {
                await Context.Message.DeleteAsync();
                var cfg = await db.GetOrCreateChannelConfigAsync(Context.Guild);
                if (!cfg.QuestionAndAnswerChannel.HasValue) return;
                var caseId = await db.CreateQnA(Context.User, Context.Guild, DateTime.UtcNow);

                var embed = new EmbedBuilder().CreateDefault(question, Context.Guild.Id)
                    .WithAuthor(new EmbedAuthorBuilder
                    {
                        IconUrl = (Context.User as SocketGuildUser).GetAvatar(),
                        Name = (Context.User as SocketGuildUser).GetName()
                    })
                    .WithFooter(new EmbedFooterBuilder {Text = $"Question ID: {caseId.Id}"})
                    .WithTimestamp(DateTimeOffset.UtcNow);

                var msg = await Context.Guild.GetTextChannel(cfg.QuestionAndAnswerChannel.Value).ReplyAsync(embed);
                caseId.MessageId = msg.Id;
                await db.SaveChangesAsync();
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().CreateDefault("Question sent!", Color.Green.RawValue).Build());
            }
        }

        [Name("Question answer")]
        [Command("qa answer", RunMode = RunMode.Async)]
        [Alias("qaa")]
        [Summary("**Require Manage Server**\nUsers with manage guild perms can answer questions sent in.")]
        [Remarks("h.qaa 1 Yes, here's your answer :pog:")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task CommentAsync(int id, [Remainder] string response)
        {
            using (var db = new DbService())
            {
                await Context.Message.DeleteAsync();
                var question = await db.QuestionAndAnswers.FindAsync(id, Context.Guild.Id);
                if (question == null) return;
                var cfg = await db.GetOrCreateChannelConfigAsync(Context.Guild);
                if (!cfg.QuestionAndAnswerChannel.HasValue)
                {
                    await Context.ReplyAsync("No QnA channel has been setup", Color.Red.RawValue);
                    return;
                }

                if (!question.MessageId.HasValue)
                {
                    await Context.ReplyAsync("Couldn't find the associated message to this question :(");
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
                    await (await suggestUser.GetOrCreateDMChannelAsync())
                        .ReplyAsync("Your question got a response!\n" +
                                    "Question:\n" +
                                    $"{embed.Description}\n" +
                                    $"Answer from {Context.User}:\n" +
                                    $"{response}", Context.Guild.Id);
                }
                catch
                {
                    /*IGNORE*/ //TODO: Add logging
                }

                await (msg as IUserMessage).ModifyAsync(x => x.Embed = embed.Build());
            }
        }

        [Name("Q&A channel")]
        [Command("qa set channel", RunMode = RunMode.Async)]
        [Alias("qac", "qachannel", "qa channel")]
        [Summary("**Require Manage Server**\nSets a channel for QnA. Leave empty channel to disable QnA.")]
        [Remarks("h.qac #general")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task SetQuestionChannelAsync(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateChannelConfigAsync(Context.Guild);
                if (cfg.QuestionAndAnswerChannel.HasValue && channel == null)
                {
                    cfg.QuestionAndAnswerChannel = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Disabled QnA channel", Color.Green.RawValue);
                    return;
                }

                if (channel == null) channel = Context.Channel as ITextChannel;

                cfg.QuestionAndAnswerChannel = channel?.Id;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"All questions will now be sent to {channel.Mention} !",
                    Color.Green.RawValue);
            }
        }
    }
}