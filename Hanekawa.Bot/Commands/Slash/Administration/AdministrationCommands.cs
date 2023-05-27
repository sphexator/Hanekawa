using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Qmmands;

namespace Hanekawa.Bot.Commands.Slash.Administration;

public class AdministrationCommands : DiscordApplicationGuildModuleBase
{
    [SlashCommand("")]
    [Description("")]
    public async Task<DiscordInteractionResponseCommandResult> BanAsync()
    {
        
        return Response("yes");
    }
    
    [SlashCommand("")]
    [Description("")]
    public async Task<DiscordInteractionResponseCommandResult> UnbanAsync()
    {
        return Response("yes");
    }
    
    [SlashCommand("")]
    [Description("")]
    public async Task<DiscordInteractionResponseCommandResult> KickAsync()
    {
        return Response("yes");
    }
    
    [SlashCommand("")]
    [Description("")]
    public async Task<DiscordInteractionResponseCommandResult> MuteAsync()
    {
        return Response("yes");
    }
    
    [SlashCommand("")]
    [Description("")]
    public async Task<DiscordInteractionResponseCommandResult> UnmuteAsync()
    {
        return Response("yes");
    }
    
    [SlashCommand("")]
    [Description("")]
    public async Task<DiscordInteractionResponseCommandResult> WarnAsync()
    {
        return Response("yes");
    }
    
    [SlashCommand("")]
    [Description("")]
    public async Task<DiscordInteractionResponseCommandResult> VoidWarnAsync()
    {
        return Response("yes");
    }
    
    [SlashCommand("")]
    [Description("")]
    public async Task<DiscordInteractionResponseCommandResult> WarnsAsync()
    {
        return Response("yes");
    }
    
    [SlashCommand("")]
    [Description("")]
    public async Task<DiscordInteractionResponseCommandResult> ClearWarnsAsync()
    {
        return Response("yes");
    }
    
    [SlashCommand("")]
    [Description("")] 
    public async Task<DiscordInteractionResponseCommandResult> Prune(int messageCount)
    {
        return Response("yes");
    }
}