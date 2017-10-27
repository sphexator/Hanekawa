using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Preconditions;
using Jibril.Data.Variables;
using Jibril.Modules.Suggestion.Services;
using Jibril.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jibril.Preconditions;

namespace Jibril.Modules.Suggestion
{
    public class Suggestion : InteractiveBase
    {
        [Command("suggest", RunMode = RunMode.Async)]
        [Alias("Suggest")]
        [RequireRole(341622220050792449)]
        [RequiredChannel(339383206669320192)]
        public async Task ServerSuggestiong([Remainder] string content = null)
        {
            var user = Context.User;
            var alterID = Context.User as IGuildUser;
            var confirm = EmbedGenerator.DefaultEmbed($"Suggestion sent to server requests", Colours.OKColour);
            await ReplyAndDeleteAsync("", false, confirm.Build(), TimeSpan.FromSeconds(15));
            await Context.Message.DeleteAsync();

            var time = DateTime.Now;
            var guild = Context.Guild as SocketGuild;
            var sc = guild.GetChannel(342519715215835136) as ITextChannel;

            await Task.Delay(100);

            SuggestionDB.AddSuggestion(Context.User, time);
            var suggestionNr = SuggestionDB.GetSuggestionID(time);

            EmbedBuilder embed = new EmbedBuilder();
            EmbedAuthorBuilder author = new EmbedAuthorBuilder();
            EmbedFooterBuilder footer = new EmbedFooterBuilder();

            embed.Color = new Color(Colours.DefaultColour);
            embed.Description = $"{content}";

            author.WithIconUrl(Context.User.GetAvatarUrl());
            author.WithName(Context.User.Username);

            footer.WithText($"Suggestion ID: {suggestionNr[0]}");

            embed.WithAuthor(author);
            embed.WithFooter(footer);
            try
            {
                var suggestMsg = await sc.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
                SuggestionDB.UpdateSuggestion(suggestMsg.Id.ToString(), suggestionNr[0]);

                await Task.Delay(260);
                await suggestMsg.AddReactionAsync(new Emoji("👍")).ConfigureAwait(false);
                await Task.Delay(260);
                await suggestMsg.AddReactionAsync(new Emoji("👎")).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.Write(e);
                await ReplyAsync("Something went wrong");
            }
        }

        [Command("denyrequest", RunMode = RunMode.Async)]
        [Alias("dr")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DenyRequest(uint casenr, [Remainder]string reason = null)
        {
            var msgIDString = SuggestionDB.SuggestionMessage(casenr);
            ulong msgID = Convert.ToUInt64(msgIDString[0]);
            var guild = Context.Guild as SocketGuild;
            var ch = guild.GetChannel(342519715215835136) as ITextChannel;
            var updMsg = await ch.GetMessageAsync(msgID) as IUserMessage;
            var oldMsg = updMsg.Embeds.FirstOrDefault();

            EmbedBuilder updEmbed = new EmbedBuilder();
            EmbedAuthorBuilder updAuthor = new EmbedAuthorBuilder();
            EmbedFooterBuilder footer = new EmbedFooterBuilder();

            updEmbed.Color = new Color(Colours.FailColour);
            updEmbed.Description = oldMsg.Description;
            updEmbed.Title = oldMsg.Title;
            updEmbed.AddField(x =>
            {
                x.Name = $"{Context.User.Username}";
                if (reason == null)
                {
                    x.Value = "No reason provided";
                }
                else
                {
                    x.Value = $"{reason}";
                }
                x.IsInline = false;
            });

            updAuthor.WithIconUrl(oldMsg.Author.Value.IconUrl);
            updAuthor.WithName(oldMsg.Author.Value.Name);

            footer.WithText(oldMsg.Footer.Value.Text);

            updEmbed.WithAuthor(updAuthor);
            updEmbed.WithFooter(footer);
            await Context.Message.DeleteAsync();
            await updMsg.ModifyAsync(m => m.Embed = updEmbed.Build());
        }

        [Command("approverequest", RunMode = RunMode.Async)]
        [Alias("ar")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ApproveRequest(uint casenr, [Remainder] string reason = null)
        {
            var msgIDString = SuggestionDB.SuggestionMessage(casenr);
            ulong msgID = Convert.ToUInt64(msgIDString[0]);
            var guild = Context.Guild as SocketGuild;
            var ch = guild.GetChannel(342519715215835136) as ITextChannel;
            var updMsg = await ch.GetMessageAsync(msgID) as IUserMessage;
            var oldMsg = updMsg.Embeds.FirstOrDefault();

            EmbedBuilder updEmbed = new EmbedBuilder();
            EmbedAuthorBuilder updAuthor = new EmbedAuthorBuilder();
            EmbedFooterBuilder footer = new EmbedFooterBuilder();

            updEmbed.Color = new Color(Colours.OKColour);
            updEmbed.Description = oldMsg.Description;
            updEmbed.Title = oldMsg.Title;
            updEmbed.AddField(x =>
            {
                x.Name = $"{Context.User.Username}";
                if (reason == null)
                {
                    x.Value = "No reason provided";
                }
                else
                {
                    x.Value = $"{reason}";
                }
                x.IsInline = false;
            });

            updAuthor.WithIconUrl(oldMsg.Author.Value.IconUrl);
            updAuthor.WithName(oldMsg.Author.Value.Name);

            footer.WithText(oldMsg.Footer.Value.Text);

            updEmbed.WithAuthor(updAuthor);
            updEmbed.WithFooter(footer);

            await Context.Message.DeleteAsync();
            await updMsg.ModifyAsync(m => m.Embed = updEmbed.Build());
        }
    }
}
