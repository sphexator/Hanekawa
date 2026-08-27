using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Hanekawa.Application.Handlers.Commands.Boost;
using Hanekawa.Application.Interfaces;
using Hanekawa.Bot.Commands.Checks;
using Hanekawa.Bot.Commands.Metas;
using Hanekawa.Bot.Mapper;
using Hanekawa.Entities;
using Hanekawa.Entities.Discord;
using Hanekawa.Localize;
using Qmmands;

namespace Hanekawa.Bot.Commands.Slash.Boost;

[SlashGroup(SlashGroupName.Boost)]
[RequireModule(ModuleName.Boost)]
public class BoostCommands(IMetrics metrics) : DiscordApplicationGuildModuleBase
{
    [SlashCommand(Metas.Boost.Config)]
    [Description("List all registered boost actions")]
    public async Task<DiscordInteractionResponseCommandResult> ListAsync()
    {
        using var _ = metrics.All<BoostCommands>();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IBoostCommandService>();
        var response = await service.ListAsync(Context.GuildId);
        if (response is null)
        {
            return Response(Localization.NoFound_BoostActions);
        }

        List<(string, string)> values = [];
        foreach (var x in response.Value.BoostConfig.GetType().GetProperties())
        {
            var value = x.GetValue(response.Value.BoostConfig);
            if (value != null)
            {
                values.Add((x.Name, value.ToString()));
            }
        }

        var fields = new List<EmbedField>();
        for (var i = 0; i < values.Count; i++)
        {
            var x = values[i];
            fields.Add(new EmbedField(x.Item1, string.IsNullOrWhiteSpace(x.Item2)
                ? "NaN"
                : x.Item2,
                true));
        }

        var embed = new Embed
        {
            Title = Localization.BoostConfig,
            Content = string.Empty,
            Fields = fields
        };

        return Response(embed.ToLocalEmbed());
    }
}