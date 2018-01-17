using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Modules.Administration.Services;

namespace Jibril.Modules.Administration
{
    public class ActionReason : InteractiveBase
    {
        [Command("reason", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task ApplyReason(uint id, [Remainder] string reason)
        {
            var msgIdStr = AdminDb.ActionCaseList(id);
            var msgId = Convert.ToUInt64(msgIdStr[0].Msgid);
            if (Context.Guild.Channels.FirstOrDefault(x => x.Id == 339381104534355970) is ITextChannel ch)
            {
                var updMsg = await ch.GetMessageAsync(msgId) as IUserMessage;
                var oldMsg = updMsg?.Embeds.FirstOrDefault();

                if (oldMsg != null)
                {
                    var author = new EmbedAuthorBuilder
                    {
                        Name = oldMsg.Author?.Name,
                        Url = oldMsg.Author?.IconUrl
                    };
                    var footer = new EmbedFooterBuilder
                    {
                        Text = oldMsg.Footer?.Text
                    };
                    var updEmbed = new EmbedBuilder
                    {
                        Author = author,
                        Footer = footer,
                        Color = oldMsg.Color,
                    };
                    var userField = oldMsg.Fields.FirstOrDefault(x => x.Name == "User");
                    var length = oldMsg.Fields.First(x => x.Name == "Length");
                    updEmbed.AddField(x =>
                    {
                        x.Name = userField.Name;
                        x.Value = userField.Value;
                        x.IsInline = userField.Inline;
                    });
                    updEmbed.AddField(x =>
                    {
                        x.Name = "Moderator";
                        x.Value = $"{Context.User.Username}";
                        x.IsInline = true;
                    });
                    try
                    {
                        {
                            updEmbed.AddField(x =>
                            {
                                x.Name = length.Name;
                                x.Value = length.Value;
                                x.IsInline = length.Inline;
                            });
                        }
                    }
                    catch {/*ignore*/}
                    updEmbed.AddField(x =>
                    {
                        x.Name = "Reason";
                        x.Value = reason != null ? $"{reason}" : "No Reason Provided";
                        x.IsInline = true;
                    });
                    await Context.Message.DeleteAsync();
                    await updMsg.ModifyAsync(m => m.Embed = updEmbed.Build());
                }
            }
        }
    }
}