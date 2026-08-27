using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Bot.Commands.Metas;
using Hanekawa.Bot.Mapper;
using Hanekawa.Entities;
using Hanekawa.Entities.Discord;
using Hanekawa.Localize;
using Qmmands;

namespace Hanekawa.Bot.Commands.Slash.Setting;

[SlashGroup(SlashGroupName.Module)]
[RequireAuthorPermissions(Permissions.ManageGuild)]
public class ModuleCommands(IMetrics metrics) : DiscordApplicationGuildModuleBase
{
    [SlashCommand(ModuleCommandName.List)]
    [Description("List all modules and their state")]
    public async Task<DiscordInteractionResponseCommandResult> List()
    {
        using var _ = metrics.All<ModuleCommands>();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IModuleService>();
        var modules = await service.GetModulesAsync(Context.GuildId, Context.CancellationToken);

        var fields = new List<EmbedField>();
        foreach (var module in modules)
        {
            fields.Add(new EmbedField(module.Name, module.Enabled ? "Enabled" : "Disabled", true));
        }

        var embed = new Embed
        {
            Title = Localization.ModuleListTitle,
            Content = string.Empty,
            Fields = fields
        };
        return Response(embed.ToLocalEmbed());
    }

    [SlashCommand(ModuleCommandName.Enable)]
    [Description("Enable a module")]
    public Task<DiscordInteractionResponseCommandResult> Enable(string module)
        => SetAsync(module, true);

    [SlashCommand(ModuleCommandName.Disable)]
    [Description("Disable a module")]
    public Task<DiscordInteractionResponseCommandResult> Disable(string module)
        => SetAsync(module, false);

    [AutoComplete(ModuleCommandName.Enable)]
    [AutoComplete(ModuleCommandName.Disable)]
    public Task AutoCompleteModule(AutoComplete<string> module)
    {
        if (!module.IsFocused) return Task.CompletedTask;
        module.Choices?.AddRange(ModuleName.All);
        return Task.CompletedTask;
    }

    private async Task<DiscordInteractionResponseCommandResult> SetAsync(string module, bool enabled)
    {
        using var _ = metrics.All<ModuleCommands>();
        var name = ModuleName.All.FirstOrDefault(x =>
            string.Equals(x, module, StringComparison.OrdinalIgnoreCase));
        if (name is null) return Response(string.Format(Localization.ModuleUnknown, module));

        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IModuleService>();
        await service.SetEnabledAsync(Context.GuildId, name, enabled, Context.CancellationToken);
        return Response(string.Format(enabled
            ? Localization.ModuleEnabled
            : Localization.ModuleDisabled, name));
    }
}
