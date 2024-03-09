using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Commands;
using Hanekawa.Bot.Mapper;
using Qmmands;

namespace Hanekawa.Bot.Commands.Slash.Club;

[SlashGroup("club")]
public class ClubCommands(IMetrics metrics) : DiscordApplicationGuildModuleBase
{
    [SlashCommand("create")]
    [Description("Create a club")]
    public async Task<DiscordInteractionResponseCommandResult> Create(string name, string description)
    {
        using var _ = metrics.All<ClubCommands>();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IClubCommandService>();
        var response = await service.Create(Context.GuildId, name, description, Context.AuthorId);
        return Response(response.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand("delete")]
    [Description("Delete a club")]
    [RequireAuthorPermissions(Permissions.ManageGuild)]
    public async Task<DiscordInteractionResponseCommandResult> Delete(string name)
    {
        using var _ = metrics.All<ClubCommands>();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IClubCommandService>();
        var response = await service.Delete(Context.GuildId, name, Context.AuthorId);
        return Response(response.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand("list")]
    [Description("List all clubs")]
    public async Task<DiscordInteractionResponseCommandResult> List()
    {
        using var _ = metrics.All<ClubCommands>();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IClubCommandService>();
        var response = await service.List(Context.GuildId);
        return Response(response.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand("join")]
    [Description("Join a club")]
    public async Task<DiscordInteractionResponseCommandResult> Join(string name)
    {
        using var _ = metrics.All<ClubCommands>();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IClubCommandService>();
        var response = await service.Join(Context.GuildId, name, Context.AuthorId);
        return Response(response.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand("leave")]
    [Description("Leave a club")]
    public async Task<DiscordInteractionResponseCommandResult> Leave(string name)
    {
        using var _ = metrics.All<ClubCommands>();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IClubCommandService>();
        var response = await service.Leave(Context.GuildId, name, Context.AuthorId);
        return Response(response.ToLocalInteractionMessageResponse());
    }

    [SlashCommand("info")]
    [Description("Get club info")]
    public async Task<DiscordInteractionResponseCommandResult> Info(string name)
    {
        using var _ = metrics.All<ClubCommands>();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IClubCommandService>();
        var response = await service.Info(Context.GuildId, name);
        return Response(response.ToLocalInteractionMessageResponse());
    }
}