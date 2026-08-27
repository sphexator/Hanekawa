using Disqord.Bot.Commands;
using Hanekawa.Application.Interfaces.Services;
using Qmmands;
using IResult = Qmmands.IResult;

namespace Hanekawa.Bot.Commands.Checks;

/// <summary>
/// Requires the specified module to be enabled for the guild.
/// </summary>
public class RequireModuleAttribute(string module) : DiscordGuildCheckAttribute
{
    public override async ValueTask<IResult> CheckAsync(IDiscordGuildCommandContext context)
    {
        await using var scope = context.Services.CreateAsyncScope();
        var modules = scope.ServiceProvider.GetRequiredService<IModuleService>();
        var enabled = await modules.IsEnabledAsync(context.GuildId.RawValue, module, context.CancellationToken);
        return enabled
            ? Qmmands.Results.Success
            : Qmmands.Results.Failure($"The {module} module is disabled in this server.");
    }
}
