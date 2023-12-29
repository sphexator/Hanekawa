using System.Runtime.InteropServices;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Hanekawa.Application;
using Hanekawa.Application.Interfaces.Commands;
using Hanekawa.Entities.Levels;
using Hanekawa.Localize;
using Qmmands;

namespace Hanekawa.Bot.Commands.Slash.Setting;

[SlashGroup("level")]
[RequireAuthorPermissions(Permissions.ManageGuild)]
public class LevelCommands : DiscordApplicationGuildModuleBase
{
    private readonly Metrics<LevelCommands> _metrics;
    
    public LevelCommands(Metrics<LevelCommands> metrics)
        => _metrics = metrics;

    [SlashCommand("add")]
    [Description("Add a level role")] 
    public async Task<DiscordInteractionResponseCommandResult> Add(int level, IRole role)
    {
        using var _ = _metrics.MeasureDuration();
        _metrics.IncrementCounter();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ILevelCommandService>();
        await service.AddAsync(Context.GuildId, role.Id, level, Context.CancellationToken);
        return Response();
    }
    
    [SlashCommand("remove")]
    [Description("Remove a level role")]
    public async Task<DiscordInteractionResponseCommandResult> Remove(IRole role)
    {
        using var _ = _metrics.MeasureDuration();
        _metrics.IncrementCounter();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ILevelCommandService>();
        await service.RemoveAsync(Context.GuildId, role.Id, Context.CancellationToken);
        return Response();
    }
    
    [SlashCommand("list")]
    [Description("List all level roles")]
    public async Task<IDiscordCommandResult> List()
    {
        using var _ = _metrics.MeasureDuration();
        _metrics.IncrementCounter();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ILevelCommandService>();
        var response = await service.ListAsync(Context.GuildId, Context.CancellationToken);
        if (response.Count == 0) return Response(Localization.NoRolesFound);
        return Pages(BuildPages(response));
    }
    
    [SlashCommand("modify")]
    [Description("Modify a level role")]
    public async Task<DiscordInteractionResponseCommandResult> Modify(int level, IRole role)
    {
        using var _ = _metrics.MeasureDuration();
        _metrics.IncrementCounter();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ILevelCommandService>();
        await service.ModifyAsync(Context.GuildId, role.Id, level, Context.CancellationToken);
        return Response();
    }
    
    [AutoComplete("add")]
    public async Task AutoCompleteAdd(AutoComplete<int> level, AutoComplete<IRole> role)
    {
        if (!role.IsFocused) return;
        if(Context.CancellationToken.IsCancellationRequested) return;
        role.Choices?.AddRange(Bot.GetGuild(Context.GuildId)?.Roles.Values ?? throw new InvalidOperationException());
    }
    
    [AutoComplete("remove")]
    public async Task AutoCompleteRemove(AutoComplete<IRole> role)
    {
        if (!role.IsFocused) return;
        if(Context.CancellationToken.IsCancellationRequested) return;
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ILevelCommandService>();
        
    }
    
    private static IEnumerable<Page> BuildPages(List<LevelReward> roles)
    {
        var result = new List<Page>();
        var span = CollectionsMarshal.AsSpan(roles);
        for (var i = 0; i < span.Length;)
        {
            var page = new Page();
            for (var j = 0; j < 5; j++)
            {
                if(i >= span.Length) break;
                var entry = span[i];
                if(roles[i].Money != null) page.Content += $"Level {entry.Level}: {entry.Money} coins\n";
                else page.Content += $"Level {roles[i].Level}: <@&{entry.RoleId}>\n";
                i++;
            }
            result.Add(page);
        }

        return result;
    }
}