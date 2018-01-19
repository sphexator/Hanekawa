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
    public class Suggestion : InteractiveBase
    {
        [Command("suggest", RunMode = RunMode.Async)]
        [Alias("suggestion")]
        [RequireRole(341622220050792449)]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task ServerSuggestiong([Remainder] string content)
        {
            try
            {
                var confirm = EmbedGenerator.DefaultEmbed($"Suggestion sent to server requests", Colours.OkColour);
                await ReplyAndDeleteAsync("", false, confirm.Build(), TimeSpan.FromSeconds(15));
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

                    Emote.TryParse("<:yes:401458712805638144>", out var yesEmote);
                    Emote.TryParse("<:no:401458713195708416>", out var noEmote);
                    IEmote iemoteYes = yesEmote;
                    IEmote iemoteNo = noEmote;
                    await Task.Delay(260);
                    await suggestMsg.AddReactionAsync(iemoteYes);
                    await Task.Delay(260);
                    await suggestMsg.AddReactionAsync(iemoteNo);
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

        [Command("denyrequest", RunMode = RunMode.Async)]
        [Alias("dr")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DenyRequest(uint casenr, [Remainder] string reason = null)
        {
            var msgIdString = SuggestionDB.SuggestionMessage(casenr);
            var msgId = Convert.ToUInt64(msgIdString[0]);
            var guild = Context.Guild;
            var ch = guild.TextChannels.First(x => x.Id == 342519715215835136);
            var updMsg = await ch.GetMessageAsync(msgId) as IUserMessage;
            var oldMsg = updMsg?.Embeds.FirstOrDefault();

            if (oldMsg == null)
            {
                Console.Write("updmsg is null");
                return;
            }

            var updAuthor = new EmbedAuthorBuilder
            {
                IconUrl = oldMsg.Author?.IconUrl,
                Name = oldMsg.Author?.Name
            };
            var footer = new EmbedFooterBuilder
            {
                Text = oldMsg.Footer?.Text
            };
            var updEmbed = new EmbedBuilder
            {
                Color = new Color(Colours.FailColour),
                Description = oldMsg.Description,
                Title = oldMsg.Title,
                Author = updAuthor,
                Footer = footer
            };
            updEmbed.AddField(x =>
            {
                x.Name = $"{Context.User.Username}";
                x.Value = reason == null ? "No reason provided" : $"{reason}";
                x.IsInline = false;
            });

            await Context.Message.DeleteAsync();
            await updMsg.ModifyAsync(m => m.Embed = updEmbed.Build());
        }

        [Command("approverequest", RunMode = RunMode.Async)]
        [Alias("ar")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ApproveRequest(uint casenr, [Remainder] string reason = null)
        {
            var msgIdString = SuggestionDB.SuggestionMessage(casenr);
            var msgId = Convert.ToUInt64(msgIdString[0]);
            var guild = Context.Guild;
            var ch = guild.TextChannels.First(x => x.Id == 342519715215835136);
            var updMsg = await ch.GetMessageAsync(msgId) as IUserMessage;
            var oldMsg = updMsg.Embeds.FirstOrDefault();

            if (oldMsg == null)
            {
                Console.Write("updmsg is null");
                return;
            }

            var updAuthor = new EmbedAuthorBuilder
            {
                IconUrl = oldMsg.Author?.IconUrl,
                Name = oldMsg.Author?.Name
            };
            var footer = new EmbedFooterBuilder
            {
                Text = oldMsg.Footer?.Text
            };
            var updEmbed = new EmbedBuilder
            {
                Color = new Color(Colours.OkColour),
                Description = oldMsg.Description,
                Title = oldMsg.Title,
                Author = updAuthor,
                Footer = footer
            };
            updEmbed.AddField(x =>
            {
                x.Name = $"{Context.User.Username}";
                x.Value = reason == null ? "No reason provided" : $"{reason}";
                x.IsInline = false;
            });

            await Context.Message.DeleteAsync();
            await updMsg.ModifyAsync(m => m.Embed = updEmbed.Build());
        }

        [Command("reviewRequest", RunMode = RunMode.Async)]
        [Alias("rr")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ReviewRequest(uint casenr, [Remainder] string reason = null)
        {
            var msgIdString = SuggestionDB.SuggestionMessage(casenr);
            var msgId = Convert.ToUInt64(msgIdString[0]);
            var guild = Context.Guild;
            var ch = guild.TextChannels.First(x => x.Id == 342519715215835136);
            var updMsg = await ch.GetMessageAsync(msgId) as IUserMessage;
            var oldMsg = updMsg?.Embeds.FirstOrDefault();

            if (oldMsg == null)
            {
                Console.Write("updmsg is null");
                return;
            }

            var updAuthor = new EmbedAuthorBuilder
            {
                IconUrl = oldMsg.Author?.IconUrl,
                Name = oldMsg.Author?.Name
            };
            var footer = new EmbedFooterBuilder
            {
                Text = oldMsg.Footer?.Text
            };
            var updEmbed = new EmbedBuilder
            {
                Color = new Color(Colours.ReviewColour),
                Description = oldMsg.Description,
                Title = oldMsg.Title,
                Author = updAuthor,
                Footer = footer
            };
            updEmbed.AddField(x =>
            {
                x.Name = $"{Context.User.Username}";
                x.Value = reason == null ? "Under review." : $"{reason}";
                x.IsInline = false;
            });
            await Context.Message.DeleteAsync();
            await updMsg.ModifyAsync(m => m.Embed = updEmbed.Build());
        }
    }
}