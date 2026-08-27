using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Application.Handlers.Services.Warnings;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Commands;
using Hanekawa.Application.Services;
using Hanekawa.Bot.Commands.Checks;
using Hanekawa.Bot.Commands.Metas;
using Hanekawa.Bot.Mapper;
using Hanekawa.Entities;
using Qmmands;

namespace Hanekawa.Bot.Commands.Slash.Administration;

[Name("Administration")]
[Description("Administration commands")]
[RequireModule(ModuleName.Administration)]
public class AdministrationCommands(IMetrics metrics) : DiscordApplicationGuildModuleBase
{
    [SlashCommand(Admin.Ban)]
    [RequireBotPermissions(Permissions.BanMembers)]
    [RequireAuthorPermissions(Permissions.BanMembers)]
    [Description("Bans a user from the server")]
    public async Task<DiscordInteractionResponseCommandResult> BanAsync(AutoComplete<IMember> member, string reason)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var user = member.Argument.Value;
        await using var scope = Bot.Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<IAdministrationCommandService>()
            .BanUserAsync(user.ToDiscordMember(), Context.AuthorId, reason);
        return Response(result.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand(Admin.Unban)]
    [RequireBotPermissions(Permissions.BanMembers)]
    [RequireAuthorPermissions(Permissions.BanMembers)]
    [Description("Unbans a user from the server")]
    public async Task<DiscordInteractionResponseCommandResult> UnbanAsync(AutoComplete<IMember> member, string reason)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var user = member.Argument.Value;
        await using var scope = Bot.Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<IAdministrationCommandService>()
            .UnbanUserAsync(user.ToGuild(),user.Id, Context.AuthorId.RawValue, reason);
        return Response(result.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand(Admin.Kick)]
    [RequireBotPermissions(Permissions.BanMembers)]
    [RequireAuthorPermissions(Permissions.BanMembers)]
    [Description("Kick a user from the server")]
    public async Task<DiscordInteractionResponseCommandResult> KickAsync(AutoComplete<IMember> member, string reason)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var user = member.Argument.Value;
        await using var scope = Bot.Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<IAdministrationCommandService>()
            .KickUserAsync(user.ToDiscordMember(), Context.AuthorId, reason);
        return Response(result.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand(Admin.Mute)]
    [RequireBotPermissions(Permissions.MuteMembers)]
    [RequireAuthorPermissions(Permissions.MuteMembers)]
    [Description("Mutes a user in the server")]
    public async Task<DiscordInteractionResponseCommandResult> MuteAsync(AutoComplete<IMember> member, 
        TimeSpan duration, string reason)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var user = member.Argument.Value;
        await using var scope = Bot.Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<IAdministrationCommandService>()
            .MuteUserAsync(user.ToDiscordMember(), Context.AuthorId, reason, duration);
        return Response(result.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand(Admin.Unmute)]
    [RequireBotPermissions(Permissions.MuteMembers)]
    [RequireAuthorPermissions(Permissions.MuteMembers)]
    [Description("Un-mutes a user in the server")]
    public async Task<DiscordInteractionResponseCommandResult> UnmuteAsync(AutoComplete<IMember> member, string reason)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var user = member.Argument.Value;
        await using var scope = Bot.Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<IAdministrationCommandService>()
            .UnmuteUserAsync(user.ToDiscordMember(), Context.AuthorId, reason);
        return Response(result.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand(Admin.Warn)]
    [RequireAuthorPermissions(Permissions.MuteMembers)]
    [Description("Warns a user in the server")]
    public async Task<DiscordInteractionResponseCommandResult> WarnAsync(AutoComplete<IMember> member, string reason)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var user = member.Argument.Value;
        var response = await Bot.Services.GetRequiredService<IRequestDispatcher>()
            .SendAsync<WarningReceived, Response<Message>>(new WarningReceived(user.ToDiscordMember(), reason, Context.AuthorId));
        return Response(response.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand(Admin.VoidWarn)]
    [RequireAuthorPermissions(Permissions.BanMembers)]
    [Description("Voids a warn from a user in the server")]
    public async Task<DiscordInteractionResponseCommandResult> VoidWarnAsync(AutoComplete<IMember> member)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var user = member.Argument.Value;
        var response = await Bot.Services.GetRequiredService<IRequestDispatcher>()
            .SendAsync<WarningClear, Response<Message>>(new WarningClear(user.ToDiscordMember(), Context.AuthorId, "Voided by moderator"));
        return Response(response.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand(Admin.WarnLog)]
    [RequireAuthorPermissions(Permissions.MuteMembers)]
    [Description("List all warnings from a user in the server")]
    public async Task<DiscordMenuCommandResult> WarnsAsync(AutoComplete<IMember> member)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var user = member.Argument.Value;
        var response = await Bot.Services.GetRequiredService<IRequestDispatcher>()
            .SendAsync<WarningList, Response<Pagination<Message>>>(new WarningList(user.GuildId, user.Id));
        return Pages(response.ToPages());
    }
    
    [SlashCommand(Admin.ClearWarns)]    
    [RequireAuthorPermissions(Permissions.BanMembers)]
    [Description("Clears all warnings from a user in the server")]
    public async Task<DiscordInteractionResponseCommandResult> ClearWarnsAsync(AutoComplete<IMember> member)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var user = member.Argument.Value;
        var response = await Bot.Services.GetRequiredService<IRequestDispatcher>()
            .SendAsync<WarningClear, Response<Message>>(new WarningClear(user.ToDiscordMember(), Context.AuthorId, 
                "Cleared by moderator", true));
        return Response(response.ToLocalInteractionMessageResponse());
    }
    
    [SlashCommand(Admin.Prune)]
    [RequireBotPermissions(Permissions.ManageMessages)]
    [RequireAuthorPermissions(Permissions.ManageMessages)]
    [Description("Prunes a number of messages from a channel")] 
    public async Task<DiscordInteractionResponseCommandResult> Prune(int messageCount = 100)
    {
        using var _ = metrics.All<AdministrationCommands>();
        var channel = Bot.GetChannel(Context.GuildId, Context.ChannelId);
        if (channel is not ITextChannel textChannel) return Response("This command can only be used in text channels");
        var messagesAsync = await textChannel.FetchMessagesAsync(messageCount);
        var messageIds = new ulong[messagesAsync.Count];

        for (var i = 0; i < messagesAsync.Count; i++)
        {
            var msg = messagesAsync[i];
            if((msg as IUserMessage)!.CreatedAt() < DateTimeOffset.UtcNow.AddDays(-14)) continue;
            messageIds[i] = msg.Id.RawValue;
        }

        await using var scope = Bot.Services.CreateAsyncScope();
        var result= await scope.ServiceProvider.GetRequiredService<IAdministrationCommandService>()
            .PruneAsync(Context.GuildId, Context.ChannelId, 
                messageIds, Context.AuthorId,
                "");
        return Response(result.ToLocalInteractionMessageResponse());
    }
}