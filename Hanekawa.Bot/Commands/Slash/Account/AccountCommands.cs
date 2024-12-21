using Disqord;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Disqord.Gateway;
using Hanekawa.Application.Handlers.Commands.Account;
using Hanekawa.Bot.Mapper;
using Qmmands;

namespace Hanekawa.Bot.Commands.Slash.Account;

[Name("Account")]
public class AccountCommands(IServiceProvider provider) : DiscordApplicationModuleBase
{
    [SlashCommand(Metas.Account.Rank)]
    [Description("Shows the rank of a user")]
    public Task<DiscordInteractionResponseCommandResult> RankAsync(IMember user)
    {
        throw new NotImplementedException();
    }

    [SlashCommand(Metas.Account.Wallet)]
    [Description("Shows the wallet of a user")]
    public Task<DiscordInteractionResponseCommandResult> WalletAsync()
    {
        throw new NotImplementedException();
    }

    [SlashCommand(Metas.Account.Profile)]
    [Description("Shows the profile of a user")]
    public async Task<DiscordInteractionResponseCommandResult> ProfileAsync()
    {
        var service = provider.GetRequiredService<AccountCommandService>();
        var result = await service.ProfileAsync(
            Bot.GetMember(Context.GuildId!.Value, Context.Author.Id)
                .ToDiscordMember());
        result.Position = 0;
        return Response(new LocalInteractionMessageResponse()
            .WithContent($"Profile for {Context.Author.Name}")
            .WithAllowedMentions(LocalAllowedMentions.None)
            .WithAttachments(
            [
                new LocalAttachment(result, "profile.png")
            ]));
    }

    [SlashCommand(Metas.Account.Top)]
    [Description("Shows the top users")]
    public Task<DiscordInteractionResponseCommandResult> TopAsync()
    {
        throw new NotImplementedException();
    }
}