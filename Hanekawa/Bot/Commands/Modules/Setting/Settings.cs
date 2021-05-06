using Disqord;
using Disqord.Bot;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules.Setting
{
    [Name("Settings")]
    [Description("Bot settings for all services and commands")]
    [RequireBotGuildPermissions(Permission.SendMessages | Permission.EmbedLinks)]
    public class Settings : HanekawaCommandModule
    {
        
    }
}