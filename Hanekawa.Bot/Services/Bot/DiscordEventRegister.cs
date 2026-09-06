using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Application.Services;
using Hanekawa.Entities.Discord;

namespace Hanekawa.Bot.Services.Bot;

public class DiscordEventRegister(IServiceProvider service) : DiscordBotService
{
    protected override ValueTask OnMemberJoined(MemberJoinedEventArgs e) =>
        service.GetRequiredService<IEventPublisher>()
            .PublishAsync(new UserJoin(e.GuildId, e.MemberId, e.Member.Name,
                e.Member.GetGuildAvatarUrl(), e.Member.CreatedAt()));

    protected override ValueTask OnMemberLeft(MemberLeftEventArgs e)
        => service.GetRequiredService<IEventPublisher>()
            .PublishAsync(new UserLeave(e.GuildId, e.MemberId));

    protected override ValueTask OnMemberUpdated(MemberUpdatedEventArgs e)
    { 
	    if (e is { OldMember: null } ) return ValueTask.CompletedTask;
		return service.GetRequiredService<IEventPublisher>() 
			.PublishAsync(new MemberUpdated(e.GuildId, ConvertToMember(e.OldMember), ConvertToMember(e.NewMember)));
    }

    protected override ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
    {
        if (e.GuildId is null || e.Member is null) return ValueTask.CompletedTask;
        return service.GetRequiredService<IEventPublisher>()
            .PublishAsync(new MessageReceived(e.GuildId.Value, e.ChannelId, ConvertToMember(e.Member), e.MessageId, e.Message.Content, e.Message.CreatedAt()));
    }

    protected override ValueTask OnMessageDeleted(MessageDeletedEventArgs e)
    {
        if (!e.GuildId.HasValue || e.Message is null) return ValueTask.CompletedTask;
        return service.GetRequiredService<IEventPublisher>()
            .PublishAsync(new MessageDeleted(e.GuildId.Value, e.ChannelId, e.Message.Author.Id,
                e.MessageId, e.Message.Content));
    }

    protected override ValueTask OnMessagesDeleted(MessagesDeletedEventArgs e)
        => service.GetRequiredService<IEventPublisher>()
            .PublishAsync(new MessagesDeleted(e.GuildId, e.ChannelId,
                e.Messages.Select(x => x.Key.RawValue).ToArray(),
                e.MessageIds.Select(x => x.RawValue).ToArray(),
                e.Messages.Select(x => x.Value.Content).ToArray()));

    protected override ValueTask OnBanCreated(BanCreatedEventArgs e)
        => service.GetRequiredService<IEventPublisher>()
            .PublishAsync(new UserBanned(new DiscordMember
            {
                Guild = new Guild { GuildId = e.GuildId },
                Id = e.UserId,
                Username = e.User.Name,
                IsBot = e.User.IsBot,
                AvatarUrl = e.User.GetAvatarUrl()
            }));

    protected override ValueTask OnBanDeleted(BanDeletedEventArgs e)
        => service.GetRequiredService<IEventPublisher>()
            .PublishAsync(new UserUnbanned(new DiscordMember
            {
                Guild = new Guild { GuildId = e.GuildId },
                Id = e.UserId,
                Username = e.User.Name,
                IsBot = e.User.IsBot,
                AvatarUrl = e.User.GetAvatarUrl()
            }));

    protected override ValueTask OnVoiceServerUpdated(VoiceServerUpdatedEventArgs e) => base.OnVoiceServerUpdated(e);

    protected override ValueTask OnVoiceStateUpdated(VoiceStateUpdatedEventArgs e)
    {
        return service.GetRequiredService<IEventPublisher>()
            .PublishAsync(new VoiceStateUpdate(e.GuildId, e.MemberId, e.NewVoiceState.ChannelId,
                e.NewVoiceState.SessionId));
    }

    protected override ValueTask OnPresenceUpdated(PresenceUpdatedEventArgs e)
    {
	    return service.GetRequiredService<IEventPublisher>()
		    .PublishAsync(new PresenceUpdated(e.GuildId, e.MemberId,
			    [.. e.NewPresence.Activities.Select(x => x.Name)]));
    }

    protected override ValueTask OnReactionAdded(ReactionAddedEventArgs e)
    {
        if (!e.GuildId.HasValue) return ValueTask.CompletedTask;
        return service.GetRequiredService<IEventPublisher>()
            .PublishAsync(new ReactionAdd(e.GuildId.Value, e.ChannelId,
                e.MessageId, e.UserId, e.Emoji.GetReactionFormat()));
    }

    protected override ValueTask OnReactionRemoved(ReactionRemovedEventArgs e)
    {
        if (!e.GuildId.HasValue) return ValueTask.CompletedTask;
        return service.GetRequiredService<IEventPublisher>()
            .PublishAsync(new ReactionRemove(e.GuildId.Value, e.ChannelId, e.MessageId, e.UserId,
                e.Emoji.GetReactionFormat()));
    }

    protected override ValueTask OnReactionsCleared(ReactionsClearedEventArgs e)
    {
        if (!e.GuildId.HasValue) return ValueTask.CompletedTask;
        return service.GetRequiredService<IEventPublisher>()
            .PublishAsync(new ReactionCleared(e.GuildId.Value, e.ChannelId, e.MessageId));
    }

    private static ulong[] ConvertRoles(IReadOnlyList<Snowflake> roles)
    {
        var toReturn = new ulong[roles.Count];
        for (var i = 0; i < roles.Count; i++)
        {
            var role = roles[i];
            toReturn[i] = role.RawValue;
        }
        return toReturn;
    }
    
    private static DiscordMember ConvertToMember(IMember member)
    {
        return new DiscordMember
        {
	        Guild = new Guild { GuildId = member.GuildId },
	        Id = member.Id,
	        RoleIds = ConvertRoles(member.RoleIds),
	        Nickname = member.Nick,
	        IsBot = member.IsBot,
	        Username = member.Name,
	        AvatarUrl = member.GetAvatarUrl(),
	        VoiceSessionId = member.GetVoiceState()?.SessionId
        };
    }
}