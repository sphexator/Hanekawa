using System.Threading.Tasks;
using Discord;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Core.Interactive;
using Qmmands;

namespace Hanekawa.Bot.Modules.Administration
{
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    public class Administration : InteractiveBase
    {
        [Name("Ban")]
        [Command("ban")]
        [Description("")]
        [Remarks("")]
        [RequireBotPermission(GuildPermission.BanMembers, GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanAsync()
        {
        }

        [Name("Kick")]
        [Command("kick")]
        [Description("")]
        [Remarks("")]
        [RequireBotPermission(GuildPermission.KickMembers, GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickAsync()
        {
        }

        [Name("Prune")]
        [Command("prune")]
        [Description("")]
        [Remarks("")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task PruneAsync()
        {
        }

        [Name("Mute")]
        [Command("mute")]
        [Description("")]
        [Remarks("")]
        [RequireBotPermission(GuildPermission.ManageMessages, GuildPermission.ManageRoles, GuildPermission.MuteMembers)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task MuteAsync()
        {
        }

        [Name("Soft Ban")]
        [Command("softban")]
        [Description("")]
        [Remarks("")]
        [RequireBotPermission(GuildPermission.ManageMessages, GuildPermission.ManageRoles, GuildPermission.MuteMembers)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task SoftBanAsync()
        {
        }

        [Name("UnMute")]
        [Command("unmute")]
        [Description("")]
        [Remarks("")]
        [RequireBotPermission(GuildPermission.ManageRoles, GuildPermission.MuteMembers)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task UnMuteAsync()
        {
        }

        [Name("Warn")]
        [Command("warn")]
        [Description("")]
        [Remarks("")]
        [RequireBotPermission(GuildPermission.BanMembers, GuildPermission.KickMembers, GuildPermission.ManageRoles,
            GuildPermission.MuteMembers)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task WarnAsync()
        {
        }

        [Name("Warn Log")]
        [Command("warnlog")]
        [Description("")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task WarnLogAsync()
        {
        }

        [Name("Reason")]
        [Command("reason")]
        [Description("")]
        [Remarks("")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task ReasonAsync()
        {
        }
    }
}