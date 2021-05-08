using Disqord;
using Disqord.Bot;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules.Account
{
    [Name("Store")]
    [Description("Commands for server store")]
    [RequireBotGuildPermissions(Permission.EmbedLinks | Permission.SendMessages)]
    public class Store : HanekawaCommandModule
    {
        
    }

    [Group("Store")]
    [Name("Store Admin")]
    [Description("Commands for store management")]
    public class StoreAdmin : Store
    {
        
    }
}