using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Application.Commands.Administration;
using Hanekawa.Bot.Extensions;
using Humanizer;
using Qmmands;

namespace Hanekawa.Bot.Commands.Slash.Administration;

[Name("Administration")]
[Description("Administration commands")]
public class AdministrationCommands : DiscordApplicationGuildModuleBase
{
    private readonly IServiceProvider _serviceProvider;
    public AdministrationCommands(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    [SlashCommand("ban")]
    [RequireBotPermissions(Permissions.BanMembers)]
    [RequireAuthorPermissions(Permissions.BanMembers)]
    [Description("Bans a user from the server")]
    public async Task<DiscordInteractionResponseCommandResult> BanAsync(AutoComplete<IMember> member, string reason)
    {
        var user = member.Argument.Value;
        await _serviceProvider.GetRequiredService<AdministrationCommandService>()
            .BanUserAsync(user.ToDiscordMember(), Context.AuthorId, reason);
        return Response($"Banned {user.Mention} from {user.GetGuild()?.Name}");
    }
    
    [SlashCommand("unban")]
    [RequireBotPermissions(Permissions.BanMembers)]
    [RequireAuthorPermissions(Permissions.BanMembers)]
    [Description("Unbans a user from the server")]
    public async Task<DiscordInteractionResponseCommandResult> UnbanAsync(AutoComplete<IMember> member, string reason)
    {
        var user = member.Argument.Value;
        await _serviceProvider.GetRequiredService<AdministrationCommandService>()
            .UnbanUserAsync(user.ToGuild(),user.Id, Context.AuthorId.RawValue, reason);
        return Response($"Unbanned {user.Id} from {Context.Author.GetGuild()?.Name}");
    }
    
    [SlashCommand("kick")]
    [RequireBotPermissions(Permissions.BanMembers)]
    [RequireAuthorPermissions(Permissions.BanMembers)]
    [Description("Kick a user from the server")]
    public async Task<DiscordInteractionResponseCommandResult> KickAsync(AutoComplete<IMember> member, string reason)
    {
        var user = member.Argument.Value;
        await _serviceProvider.GetRequiredService<AdministrationCommandService>()
            .KickUserAsync(user.ToDiscordMember(), Context.AuthorId, reason);
        return Response($"Kicked {user.Name} from {user.GetGuild()?.Name}");
    }
    
    [SlashCommand("mute")]
    [RequireBotPermissions(Permissions.MuteMembers)]
    [RequireAuthorPermissions(Permissions.MuteMembers)]
    [Description("Mutes a user in the server")]
    public async Task<DiscordInteractionResponseCommandResult> MuteAsync(AutoComplete<IMember> member, 
        TimeSpan duration, string reason)
    {
        var user = member.Argument.Value;
        await _serviceProvider.GetRequiredService<AdministrationCommandService>()
            .MuteUserAsync(user.ToDiscordMember(), Context.AuthorId, reason, duration);
        return Response($"Muted {user.Mention} for {duration.Humanize()}");
    }
    
    [SlashCommand("unmute")]
    [RequireBotPermissions(Permissions.MuteMembers)]
    [RequireAuthorPermissions(Permissions.MuteMembers)]
    [Description("Un-mutes a user in the server")]
    public async Task<DiscordInteractionResponseCommandResult> UnmuteAsync(AutoComplete<IMember> member, string reason)
    {
        var user = member.Argument.Value;
        await _serviceProvider.GetRequiredService<AdministrationCommandService>()
            .UnmuteUserAsync(user.ToDiscordMember(), Context.AuthorId, reason);
        return Response($"Un-muted {user.Mention}");
    }
    
    [SlashCommand("warn")]
    [RequireAuthorPermissions(Permissions.MuteMembers)]
    [Description("Warns a user in the server")]
    public async Task<DiscordInteractionResponseCommandResult> WarnAsync(AutoComplete<IMember> member, string reason)
    {
        var user = member.Argument.Value;
        await _serviceProvider.GetRequiredService<WarningCommandService>()
            .WarnUserAsync(user.GuildId, user.Id, Context.AuthorId, reason);
        return Response($"Warned {user.Mention}");
    }
    
    [SlashCommand("voidwarn")]
    [RequireAuthorPermissions(Permissions.BanMembers)]
    [Description("Voids a warn from a user in the server")]
    public async Task<DiscordInteractionResponseCommandResult> VoidWarnAsync(AutoComplete<IMember> member)
    {
        var user = member.Argument.Value;
        await _serviceProvider.GetRequiredService<WarningCommandService>()
            .ClearUserWarnAsync(user.GuildId, user.Id, Context.AuthorId, "Voided by moderator");
        return Response("yes");
    }
    
    [SlashCommand("warnlog")]
    [RequireAuthorPermissions(Permissions.MuteMembers)]
    [Description("List all warnings from a user in the server")]
    public async Task<DiscordInteractionResponseCommandResult> WarnsAsync(AutoComplete<IMember> member)
    {
        var user = member.Argument.Value;
        var warns = await _serviceProvider.GetRequiredService<WarningCommandService>()
            .WarnsAsync(user.GuildId, user.Id);
        
        return Response("yes");
    }
    
    [SlashCommand("clearwarn")]    
    [RequireAuthorPermissions(Permissions.BanMembers)]
    [Description("Clears all warnings from a user in the server")]
    public async Task<DiscordInteractionResponseCommandResult> ClearWarnsAsync(AutoComplete<IMember> member)
    {
        var user = member.Argument.Value;
        await _serviceProvider.GetRequiredService<WarningCommandService>()
            .ClearUserWarnAsync(user.GuildId, user.Id, 
                Context.AuthorId, "Cleared by moderator", true);
        return Response($"Cleared all warnings for {user.Mention}");
    }
    
    [SlashCommand("prune")]
    [RequireBotPermissions(Permissions.ManageMessages)]
    [RequireAuthorPermissions(Permissions.ManageMessages)]
    [Description("Prunes a number of messages from a channel")] 
    public async Task<DiscordInteractionResponseCommandResult> Prune(int messageCount = 100)
    {
        var channel = Bot.GetChannel(Context.GuildId, Context.ChannelId) as ITextChannel;
        var messagesAsync = await channel.FetchMessagesAsync(messageCount);
        var messageIds = new ulong[messagesAsync.Count];

        for (var i = 0; i < messagesAsync.Count; i++)
        {
            var msg = messagesAsync[i];
            if((msg as IUserMessage)!.CreatedAt() < DateTimeOffset.UtcNow.AddDays(-14)) continue;
            messageIds[i] = msg.Id.RawValue;
        }

        await _serviceProvider.GetRequiredService<AdministrationCommandService>()
            .PruneAsync(Context.GuildId, Context.ChannelId, 
                messageIds, Context.AuthorId,
                "");
        return Response($"Pruned {messageIds.Length} messages from this channel");
    }
}