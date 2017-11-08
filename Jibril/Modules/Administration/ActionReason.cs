using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Modules.Administration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Modules.Administration
{
    public class ActionReason : ModuleBase<SocketCommandContext>
    {
        [Command("reason", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task ApplyReason(uint id, [Remainder] string reason)
        {
            var msgIdStr = AdminDb.ActionCaseMessage(id);
            ulong msgId = Convert.ToUInt64(msgIdStr[0]);
            var ch = Context.Guild.Channels.FirstOrDefault(x => x.Id == 339381104534355970) as ITextChannel;
            var updMsg = await ch.GetMessageAsync(msgId) as IUserMessage;
            var oldMsg = updMsg.Embeds.FirstOrDefault();

            EmbedBuilder updEmbed = new EmbedBuilder();
            EmbedFooterBuilder footer = new EmbedFooterBuilder();

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
                if(reason != null)
                {
                    x.Value = $"{reason}";
                }
                else
                {
                    x.Value = "No Reason Provided";
                }
                x.IsInline = true;
            });
            footer.WithText(oldMsg.Footer.Value.Text);
            footer.WithIconUrl(oldMsg.Footer.Value.IconUrl);

            updEmbed.WithFooter(footer);

            await Context.Message.DeleteAsync();
            await updMsg.ModifyAsync(m => m.Embed = updEmbed.Build());
        }
    }
}