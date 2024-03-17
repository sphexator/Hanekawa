using Disqord;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Qmmands;

namespace Hanekawa.Bot.Commands.Slash.Account;

[Name("Account")]
public class AccountCommands : DiscordApplicationModuleBase
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
    public Task<DiscordInteractionResponseCommandResult> ProfileAsync()
    {
        throw new NotImplementedException();
    }
}