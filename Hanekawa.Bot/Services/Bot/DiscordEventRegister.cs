using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Hanekawa.Application.Contracts.Discord;
using Hanekawa.Entities.Discord;
using MediatR;

namespace Hanekawa.Bot.Services.Bot;

public class DiscordEventRegister : DiscordBotService
{
    private readonly IMediator _mediator;
    public DiscordEventRegister(IMediator mediator) => _mediator = mediator;

    protected override async ValueTask OnMemberJoined(MemberJoinedEventArgs e) =>
        await _mediator.Send(new UserJoin(e.GuildId, e.MemberId, e.Member.Name,
            e.Member.GetGuildAvatarUrl(), e.Member.CreatedAt()));

    protected override async ValueTask OnMemberLeft(MemberLeftEventArgs e) 
        => await _mediator.Send(new UserLeave(e.GuildId, e.MemberId));

    protected override async ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
    {
        if (e.GuildId is null || e.Member is null) return;
        await _mediator.Send(new MessageReceived(e.GuildId.Value, e.ChannelId, new()
            {
                Guild = new () { Id = e.GuildId.Value },
                UserId = e.Member.Id,
                RoleIds = ConvertRoles(e.Member.RoleIds),
                Nickname = e.Member.Nick,
                IsBot = e.Member.IsBot,
                Username = e.Member.Name,
                AvatarUrl = e.Member.GetAvatarUrl(),
                VoiceSessionId = e.Member.GetVoiceState()?.SessionId
            }, e.MessageId, e.Message.Content, e.Message.CreatedAt()));
    }
    
    protected override async ValueTask OnMessageDeleted(MessageDeletedEventArgs e)
    {
        if (e.GuildId.HasValue is false || e.Message is null) return;
        await _mediator.Send(new MessageDeleted(e.GuildId.Value, e.ChannelId, e.Message.Author.Id, 
            e.MessageId, e.Message.Content));
    }

    protected override async ValueTask OnMessagesDeleted(MessagesDeletedEventArgs e) 
        => await _mediator.Send(new MessagesDeleted(e.GuildId, e.ChannelId,
            e.Messages.Select(x => x.Key.RawValue).ToArray(), 
            e.MessageIds.Select(x => x.RawValue).ToArray(),
            e.Messages.Select(x => x.Value.Content).ToArray()));

    protected override async ValueTask OnBanCreated(BanCreatedEventArgs e) 
        => await _mediator.Send(new UserBanned(new DiscordMember
        {
            Guild = new () { Id = e.GuildId },
            UserId = e.UserId,
            Username = e.User.Name,
            IsBot = e.User.IsBot,
            AvatarUrl = e.User.GetAvatarUrl()
        }));

    protected override async ValueTask OnBanDeleted(BanDeletedEventArgs e) 
        => await _mediator.Send(new UserUnbanned(new DiscordMember
        {
            Guild = new () { Id = e.GuildId },
            UserId = e.UserId,
            Username = e.User.Name,
            IsBot = e.User.IsBot,
            AvatarUrl = e.User.GetAvatarUrl(),
        }));

    protected override ValueTask OnVoiceServerUpdated(VoiceServerUpdatedEventArgs e)
    {
        return base.OnVoiceServerUpdated(e);
    }
    
    protected override async ValueTask OnVoiceStateUpdated(VoiceStateUpdatedEventArgs e)
    {
        await _mediator.Send(new VoiceStateUpdate(e.GuildId, e.MemberId, e.NewVoiceState.ChannelId,
            e.NewVoiceState.SessionId));
    }
    
    protected override async ValueTask OnReactionAdded(ReactionAddedEventArgs e)
    {
        if(e.GuildId.HasValue is false) return;
        await _mediator.Send(new ReactionAdd(e.GuildId.Value, e.ChannelId,
            e.MessageId, e.UserId, e.Emoji.GetReactionFormat()));
    }
    
    protected override async ValueTask OnReactionRemoved(ReactionRemovedEventArgs e)
    {
        if(e.GuildId.HasValue is false) return;
        await _mediator.Send(new ReactionRemove(e.GuildId.Value, e.ChannelId, e.MessageId, e.UserId,
            e.Emoji.GetReactionFormat()));
    }
    
    protected override async ValueTask OnReactionsCleared(ReactionsClearedEventArgs e)
    {
        if(e.GuildId.HasValue is false) return;
        await _mediator.Send(new ReactionCleared(e.GuildId.Value, e.ChannelId, e.MessageId));
    }
    
    private static HashSet<ulong> ConvertRoles(IEnumerable<Snowflake> roles)
    {
        var toReturn = new HashSet<ulong>();
        foreach (var role in roles) 
            toReturn.Add(role);
        return toReturn;
    }
}