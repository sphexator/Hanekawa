using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Application.Handlers.Commands.Administration;
using Hanekawa.Application.Handlers.Services.Warnings;
using Hanekawa.Application.Interfaces;
using Hanekawa.Bot.Mapper;
using MediatR;
using Qmmands;

namespace Hanekawa.Bot.Commands.Slash.Administration;

[Name("Administration")]
[Description("Administration commands")]
public class AdministrationCommands(IMetrics metrics) : DiscordApplicationGuildModuleBase
{
    [SlashCommand("ban")]
    [RequireBotPermissions(Permissions.BanMembers)]
    [RequireAuthorPermissions(Permissions.BanMembers)]
    [Description("Bans a user from the server")]
    public async Task<DiscordInteractionResponseCommandResult> BanAsync(AutoComplete<IMember> member, string reason)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var user = member.Argument.Value;
        var result = await Bot.Services.GetRequiredService<AdministrationCommandService>()
            .BanUserAsync(user.ToDiscordMember(), Context.AuthorId, reason);
        return Response(result.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand("unban")]
    [RequireBotPermissions(Permissions.BanMembers)]
    [RequireAuthorPermissions(Permissions.BanMembers)]
    [Description("Unbans a user from the server")]
    public async Task<DiscordInteractionResponseCommandResult> UnbanAsync(AutoComplete<IMember> member, string reason)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var user = member.Argument.Value;
        var result = await Bot.Services.GetRequiredService<AdministrationCommandService>()
            .UnbanUserAsync(user.ToGuild(),user.Id, Context.AuthorId.RawValue, reason);
        return Response(result.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand("kick")]
    [RequireBotPermissions(Permissions.BanMembers)]
    [RequireAuthorPermissions(Permissions.BanMembers)]
    [Description("Kick a user from the server")]
    public async Task<DiscordInteractionResponseCommandResult> KickAsync(AutoComplete<IMember> member, string reason)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var user = member.Argument.Value;
        var result = await Bot.Services.GetRequiredService<AdministrationCommandService>()
            .KickUserAsync(user.ToDiscordMember(), Context.AuthorId, reason);
        return Response(result.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand("mute")]
    [RequireBotPermissions(Permissions.MuteMembers)]
    [RequireAuthorPermissions(Permissions.MuteMembers)]
    [Description("Mutes a user in the server")]
    public async Task<DiscordInteractionResponseCommandResult> MuteAsync(AutoComplete<IMember> member, 
        TimeSpan duration, string reason)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var user = member.Argument.Value;
        var result = await Bot.Services.GetRequiredService<AdministrationCommandService>()
            .MuteUserAsync(user.ToDiscordMember(), Context.AuthorId, reason, duration);
        return Response(result.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand("unmute")]
    [RequireBotPermissions(Permissions.MuteMembers)]
    [RequireAuthorPermissions(Permissions.MuteMembers)]
    [Description("Un-mutes a user in the server")]
    public async Task<DiscordInteractionResponseCommandResult> UnmuteAsync(AutoComplete<IMember> member, string reason)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var user = member.Argument.Value;
        var result = await Bot.Services.GetRequiredService<AdministrationCommandService>()
            .UnmuteUserAsync(user.ToDiscordMember(), Context.AuthorId, reason);
        return Response(result.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand("warn")]
    [RequireAuthorPermissions(Permissions.MuteMembers)]
    [Description("Warns a user in the server")]
    public async Task<DiscordInteractionResponseCommandResult> WarnAsync(AutoComplete<IMember> member, string reason)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var user = member.Argument.Value;
        var response = await Bot.Services.GetRequiredService<IMediator>()
            .Send(new WarningReceived(user.ToDiscordMember(), reason, Context.AuthorId));
        return Response(response.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand("voidwarn")]
    [RequireAuthorPermissions(Permissions.BanMembers)]
    [Description("Voids a warn from a user in the server")]
    public async Task<DiscordInteractionResponseCommandResult> VoidWarnAsync(AutoComplete<IMember> member)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var user = member.Argument.Value;
        var response = await Bot.Services.GetRequiredService<IMediator>()
            .Send(new WarningClear(user.ToDiscordMember(), Context.AuthorId, "Voided by moderator"));
        return Response(response.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand("warnlog")]
    [RequireAuthorPermissions(Permissions.MuteMembers)]
    [Description("List all warnings from a user in the server")]
    public async Task<DiscordInteractionResponseCommandResult> WarnsAsync(AutoComplete<IMember> member)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var user = member.Argument.Value;
        var response = await Bot.Services.GetRequiredService<IMediator>()
            .Send(new WarningList(user.GuildId, user.Id));
        return Response(response.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand("clearwarns")]    
    [RequireAuthorPermissions(Permissions.BanMembers)]
    [Description("Clears all warnings from a user in the server")]
    public async Task<DiscordInteractionResponseCommandResult> ClearWarnsAsync(AutoComplete<IMember> member)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var user = member.Argument.Value;
        var response = await Bot.Services.GetRequiredService<IMediator>()
            .Send(new WarningClear(user.ToDiscordMember(), Context.AuthorId, 
                "Cleared by moderator", true));
        return Response(response.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand("prune")]
    [RequireBotPermissions(Permissions.ManageMessages)]
    [RequireAuthorPermissions(Permissions.ManageMessages)]
    [Description("Prunes a number of messages from a channel")] 
    public async Task<DiscordInteractionResponseCommandResult> Prune(int messageCount = 100)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var channel = Bot.GetChannel(Context.GuildId, Context.ChannelId) as ITextChannel;
        var messagesAsync = await channel.FetchMessagesAsync(messageCount);
        var messageIds = new ulong[messagesAsync.Count];

        for (var i = 0; i < messagesAsync.Count; i++)
        {
            var msg = messagesAsync[i];
            if((msg as IUserMessage)!.CreatedAt() < DateTimeOffset.UtcNow.AddDays(-14)) continue;
            messageIds[i] = msg.Id.RawValue;
        }

        var result= await Bot.Services.GetRequiredService<AdministrationCommandService>()
            .PruneAsync(Context.GuildId, Context.ChannelId, 
                messageIds, Context.AuthorId,
                "");
        return Response(result.ToLocalInteractionMessageResponse());
    }
}