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
    public class QuestionAndAnswer : InteractiveBase
    {
        private readonly DbService _db;
        public QuestionAndAnswer(DbService db)
        {
            _db = db;
        }

        [Command("question", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [Summary("Sends a question to server owners to answer (QnA)")]
        public async Task QuestionAsync([Remainder] string question)
        {
            await Context.Message.DeleteAsync();
                var cfg = await _db.GetOrCreateGuildConfig(Context.Guild);
                if (!cfg.QuestionAndAnswerChannel.HasValue) return;
                var caseId = await _db.CreateQnA(Context.User, Context.Guild, DateTime.UtcNow);

                var embed = new EmbedBuilder().CreateDefault(question)
                    .WithAuthor(new EmbedAuthorBuilder { IconUrl = (Context.User as SocketGuildUser).GetAvatar(), Name = (Context.User as SocketGuildUser).GetName() })
                    .WithFooter(new EmbedFooterBuilder { Text = $"Question ID: {caseId.Id}" })
                    .WithTimestamp(DateTimeOffset.UtcNow);

                var msg = await Context.Guild.GetTextChannel(cfg.QuestionAndAnswerChannel.Value).ReplyAsync(embed);
                caseId.MessageId = msg.Id;
                await _db.SaveChangesAsync();
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().CreateDefault("Question sent!", Color.Green.RawValue).Build());
        }

        [Command("qa answer", RunMode = RunMode.Async)]
        [Alias("qaa")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Users with manage guild perms can answer questions sent in.")]
        public async Task CommentAsync(uint id, [Remainder] string response)
        {
            await Context.Message.DeleteAsync();
                var question = await _db.QuestionAndAnswers.FindAsync(id, Context.Guild.Id);
                if (question == null) return;
                var cfg = await _db.GetOrCreateGuildConfig(Context.Guild);
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
                            $"{response}");
                }
                catch
                {
                    /*IGNORE*/ //TODO: Add logging
                }

                await (msg as IUserMessage).ModifyAsync(x => x.Embed = embed.Build());
        }

        [Command("qa set channel", RunMode = RunMode.Async)]
        [Alias("qac", "qachannel", "qa channel")]
        [Summary("Sets a channel for QnA. Leave empty channel to disable QnA.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task SetQuestionChannelAsync(ITextChannel channel = null)
        {
                var cfg = await _db.GetOrCreateGuildConfig(Context.Guild);
                if (cfg.QuestionAndAnswerChannel.HasValue && channel == null)
                {
                    cfg.QuestionAndAnswerChannel = null;
                    await _db.SaveChangesAsync();
                    await Context.ReplyAsync("Disabled QnA channel", Color.Green.RawValue);
                    return;
                }

                if (channel == null) channel = Context.Channel as ITextChannel;

                cfg.QuestionAndAnswerChannel = channel?.Id;
                await _db.SaveChangesAsync();
            await Context.ReplyAsync($"All questions will now be sent to {channel.Mention} !",
                Color.Green.RawValue);
        }
    }
}