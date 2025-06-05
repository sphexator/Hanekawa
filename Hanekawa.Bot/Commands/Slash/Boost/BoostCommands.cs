using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Hanekawa.Application.Handlers.Commands.Boost;
using Hanekawa.Application.Interfaces;
using Hanekawa.Bot.Commands.Metas;
using Hanekawa.Bot.Mapper;
using Hanekawa.Entities.Discord;
using Hanekawa.Localize;
using Qmmands;

namespace Hanekawa.Bot.Commands.Slash.Boost;

[SlashGroup(SlashGroupName.Boost)]
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

        if (response.Length is 0)
        {
            return Response(Localization.NoFound_BoostActions);
        }

        var fields = new List<EmbedField>();
        for (var i = 0; i < response.Length; i++)
        {
            var x = response[i];
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