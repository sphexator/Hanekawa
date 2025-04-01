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
    public async Task<DiscordInteractionResponseCommandResult> RankAsync(IMember user)
    {
        var service = provider.GetRequiredService<AccountCommandService>();
        var result = await service.RankAsync(user.ToDiscordMember());

        var response = new LocalInteractionMessageResponse().WithAttachments(new LocalAttachment(result, "rank.png"));
        return Response(response);
    }

    [SlashCommand(Metas.Account.Wallet)]
    [Description("Shows the wallet of a user")]
    public async Task<DiscordInteractionResponseCommandResult> WalletAsync()
    {
        var service = provider.GetRequiredService<AccountCommandService>();
        await service.GetWalletAsync((Context.Author as IMember).ToDiscordMember());

        return Response(new LocalInteractionMessageResponse()
            .WithContent($"Wallet for {Context.Author.Name}")
            .WithAllowedMentions(LocalAllowedMentions.None)
            .WithAttachments(new LocalAttachment(result, "wallet.png")));
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
    public async Task<DiscordInteractionResponseCommandResult> TopAsync()
    {
        var service = provider.GetRequiredService<AccountCommandService>();
        
        // Get top users from the service
        var topUsers = await service.GetTopUsersAsync(Context.GuildId!.Value);
        
        var embed = new LocalEmbed()
            .WithTitle("Top 10 Users")
            .WithDescription("Ranked by experience")
            .WithColor(Color.Purple); // Default to purple as requested
            
        int rank = 1;
        for (int i = 0; i < topUsers.Length; i++)
        {
            var user = topUsers[i];
            var member = Bot.GetMember(Context.GuildId!.Value, user.Id);
            string displayName = member != null ? member.Name : $"User {user.Id}";
            
            embed.AddField($"#{rank} {displayName}", 
                $"Level: {user.Level}\n" +
                $"Experience: {user.Experience}\n" +
                $"Currency: {user.Currency}", 
                false);
            
            rank++;
        }
        
        return Response(new LocalInteractionMessageResponse()
            .WithEmbeds(embed)
            .WithAllowedMentions(LocalAllowedMentions.None));
        throw new NotImplementedException();
    }
}