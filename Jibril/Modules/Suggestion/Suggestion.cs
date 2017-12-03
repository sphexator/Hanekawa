using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Data.Variables;
using Jibril.Modules.Suggestion.Services;
using Jibril.Preconditions;
using Jibril.Services.Common;

namespace Jibril.Modules.Suggestion
{
    public class Suggestion : ModuleBase<SocketCommandContext>
    {
        [Command("suggest")]
        [Alias("Suggest")]
        //[RequireRole(341622220050792449)]
        //[RequiredChannel(339383206669320192)]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task ServerSuggestiong([Remainder] string content)
        {
            try
            {
                var confirm = EmbedGenerator.DefaultEmbed($"Suggestion sent to server requests", Colours.OKColour);
                await ReplyAsync("", false, confirm.Build()/*, TimeSpan.FromSeconds(15)*/);
                await Context.Message.DeleteAsync();

                var time = DateTime.Now;
                var guild = Context.Guild;
                var sc = guild.TextChannels.First(x => x.Id == 342519715215835136);

                await Task.Delay(100);

                SuggestionDB.AddSuggestion(Context.User, time);
                var suggestionNr = SuggestionDB.GetSuggestionID(time);


                var author = new EmbedAuthorBuilder();
                var footer = new EmbedFooterBuilder();
                var embed = new EmbedBuilder
                {
                    Color = new Color(Colours.DefaultColour),
                    Description = $"{content}"
                };
                if (Context.Message.Attachments.ToString() == null)
                    embed.ImageUrl = Context.Message.Attachments.ToString();

                author.WithIconUrl(Context.User.GetAvatarUrl());
                author.WithName(Context.User.Username);

                footer.WithText($"Suggestion ID: {suggestionNr[0]}");

                embed.WithAuthor(author);
                embed.WithFooter(footer);

                try
                {
                    var suggestMsg = await sc.SendMessageAsync("", false, embed.Build());
                    SuggestionDB.UpdateSuggestion(suggestMsg.Id.ToString(), suggestionNr[0]);

                    await Task.Delay(260);
                    await suggestMsg.AddReactionAsync(new Emoji("👍"));
                    await Task.Delay(260);
                    await suggestMsg.AddReactionAsync(new Emoji("👎"));
                }
                catch (Exception e)
                {
                    Console.Write(e);
                    await ReplyAsync("Something went wrong");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Command("denyrequest")]
        [Alias("dr")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DenyRequest(uint casenr, [Remainder] string reason = null)
        {
            var msgIDString = SuggestionDB.SuggestionMessage(casenr);
            var msgID = Convert.ToUInt64(msgIDString[0]);
            var guild = Context.Guild;
            var ch = guild.GetChannel(342519715215835136) as ITextChannel;
            var updMsg = await ch.GetMessageAsync(msgID) as IUserMessage;
            var oldMsg = updMsg.Embeds.FirstOrDefault();

            var updEmbed = new EmbedBuilder();
            var updAuthor = new EmbedAuthorBuilder();
            var footer = new EmbedFooterBuilder();

            updEmbed.Color = new Color(Colours.FailColour);
            updEmbed.Description = oldMsg.Description;
            updEmbed.Title = oldMsg.Title;
            updEmbed.ImageUrl = oldMsg.Image.Value.Url;
            updEmbed.AddField(x =>
            {
                x.Name = $"{Context.User.Username}";
                if (reason == null)
                    x.Value = "No reason provided";
                else
                    x.Value = $"{reason}";
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

        [Command("approverequest")]
        [Alias("ar")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ApproveRequest(uint casenr, [Remainder] string reason = null)
        {
            var msgIDString = SuggestionDB.SuggestionMessage(casenr);
            var msgID = Convert.ToUInt64(msgIDString[0]);
            var guild = Context.Guild;
            var ch = guild.GetChannel(342519715215835136) as ITextChannel;
            var updMsg = await ch.GetMessageAsync(msgID) as IUserMessage;
            var oldMsg = updMsg.Embeds.FirstOrDefault();

            var updEmbed = new EmbedBuilder();
            var updAuthor = new EmbedAuthorBuilder();
            var footer = new EmbedFooterBuilder();

            updEmbed.Color = new Color(Colours.OKColour);
            updEmbed.Description = oldMsg.Description;
            updEmbed.Title = oldMsg.Title;
            updEmbed.AddField(x =>
            {
                x.Name = $"{Context.User.Username}";
                if (reason == null)
                    x.Value = "No reason provided";
                else
                    x.Value = $"{reason}";
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

        private static bool _AttachmentUrl(string url)
        {
            if (url.EndsWith(".png")) return true;
            if (url.EndsWith(".jpeg")) return true;
            if (url.EndsWith(".jpg")) return true;
            if (url.EndsWith(".gif")) return true;
            return false;
        }
    }
}