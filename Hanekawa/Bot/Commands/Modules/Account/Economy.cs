using Disqord;
using Disqord.Bot;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules.Account
{
    [Name("Economy")]
    [Description("Commands for user economy")]
    [RequireBotGuildPermissions(Permission.EmbedLinks | Permission.SendMessages)]
    public class Economy : HanekawaCommandModule
    {
        
    }

    [Group("Currency")]
    [Name("Currency Admin")]
    public class EconomySettings : Economy
    {
        
    }
}