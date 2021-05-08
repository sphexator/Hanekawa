using Disqord;
using Disqord.Bot;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules
{
    [Name("Suggestion")]
    [Description("Commands for suggestions")]
    [RequireBotGuildPermissions(Permission.SendMessages | Permission.EmbedLinks)]
    public class Suggestion : HanekawaCommandModule
    {
        
    }

    [Name("Suggestion Admin")]
    [Group("Suggest")]
    public class SuggestionAdmin : Suggestion
    {
        
    }
}