using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Hanekawa.Application.Handlers.Commands.Boost;
using Hanekawa.Application.Interfaces;
using Hanekawa.Bot.Commands.Metas;
using Hanekawa.Bot.Mapper;
using Hanekawa.Entities.Discord;
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
        var response = await service.List(Context.GuildId);

        if (response.Count is 0)
        {
            return Response("No boost actions found.");
        }

        var fields = new List<EmbedField>();
        for (int i = 0; i < response.Count; i++)
        {
            var x = response[i];
            fields.Add(new EmbedField(x.Item1, x.Item2.ToString() ?? "NaN", true));
        }

        var embed = new Embed
        {
            Title = "Boost configuration",
            Fields = fields
        };

        return Response(embed.ToLocalEmbed());
    }
}