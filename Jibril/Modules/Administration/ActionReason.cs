using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Modules.Administration.Services;
using Jibril.Services;

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
            var usrId = Convert.ToUInt64(msgIdStr[0].User_id);
            if (Context.Guild.Channels.FirstOrDefault(x => x.Id == 339381104534355970) is ITextChannel ch)
            {
                var updMsg = await ch.GetMessageAsync(msgId) as IUserMessage;
                var oldMsg = updMsg?.Embeds.FirstOrDefault();

                var updEmbed = new EmbedBuilder();
                var footer = new EmbedFooterBuilder();
                if (oldMsg != null)
                {
                    updEmbed.Color = oldMsg.Color;
                    updEmbed.Description = oldMsg.Description;
                    updEmbed.AddField(x =>
                    {
                        x.Name = "Moderator";
                        x.Value = $"{Context.User.Username}";
                        x.IsInline = true;
                    });
                    updEmbed.AddField(x =>
                    {
                        x.Name = "Reason";
                        x.Value = reason != null ? $"{reason}" : "No Reason Provided";
                        x.IsInline = true;
                    });
                    if (oldMsg.Footer != null)
                    {
                        footer.WithText(oldMsg.Footer.Value.Text);
                        footer.WithIconUrl(oldMsg.Footer.Value.IconUrl);
                    }
                }

                updEmbed.WithFooter(footer);
                await Context.Message.DeleteAsync();
                if (updMsg != null) await updMsg.ModifyAsync(m => m.Embed = updEmbed.Build());
            }
            var userdata = DatabaseService.UserData(usrId).FirstOrDefault();
            if (userdata?.Level > 10 && userdata.Level < 25)
            {
                await ReplyAsync("Is this a perm. ban? (Y/N)");
                var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(30));
                if (response.Content.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                {
                    AdminDb.UpdateBanPerm(usrId);
                    await response.DeleteAsync();
                }
            }
        }
    }
}