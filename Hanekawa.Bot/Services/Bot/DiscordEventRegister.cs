using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Hanekawa.Application.Contracts.Discord;
using MediatR;

namespace Hanekawa.Bot.Services.Bot;

public class DiscordEventRegister : DiscordBotService
{
    private readonly IMediator _mediator;
    public DiscordEventRegister(IMediator mediator) => _mediator = mediator;

    protected override async ValueTask OnMemberJoined(MemberJoinedEventArgs e) =>
        await _mediator.Send(new UserJoin
        {
            GuildId = e.GuildId,
            UserId = e.MemberId,
            Username = e.Member.Name,
            AvatarUrl = e.Member.GetAvatarUrl(),
            Discriminator = e.Member.Discriminator,
            CreatedAt = e.Member.CreatedAt()
        });

    protected override async ValueTask OnMemberLeft(MemberLeftEventArgs e)
    {
        await _mediator.Send(new UserLeave
        {
            GuildId = e.GuildId,
            UserId = e.MemberId
        });
    }

    protected override async ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
    {
        if (e.GuildId is null || e.Member is null) return;
        await _mediator.Send(new MessageReceived
        {
            GuildId = e.GuildId.Value,
            ChannelId = e.ChannelId,
            MessageId = e.MessageId,
            CreatedAt = e.Message.CreatedAt(),
            Member = new ()
            {
                GuildId = e.GuildId.Value,
                UserId = e.Member.Id,
                RoleIds = ConvertRoles(e.Member.RoleIds),
                Nickname = e.Member.Nick,
                IsBot = e.Member.IsBot,
                Discriminator = e.Member.Discriminator,
                Username = e.Member.Name,
                AvatarUrl = e.Member.GetAvatarUrl(),
                VoiceSessionId = e.Member.GetVoiceState()?.SessionId
            }
        });
    }
    
    protected override async ValueTask OnMessageDeleted(MessageDeletedEventArgs e)
    {
        if (e.GuildId.HasValue is false || e.Message is null) return;
        await _mediator.Send(new MessageDeleted
        {
            GuildId = e.GuildId.Value,
            ChannelId = e.ChannelId,
            AuthorId = e.Message.Author.Id.RawValue,
            MessageId = e.MessageId,
            MessageContent = e.Message.Content
        });
    }

    protected override async ValueTask OnMessagesDeleted(MessagesDeletedEventArgs e)
    {
        await _mediator.Send(new MessagesDeleted
        {
            GuildId = e.GuildId,
            ChannelId = e.ChannelId,
            AuthorId = e.Messages.Select(x => x.Value.Author.Id.RawValue).ToArray(),
            MessageContents = e.Messages.Select(x => x.Value.Content).ToArray(),
            MessageIds = e.MessageIds.Select(x => x.RawValue).ToArray()
        });
    }

    protected override async ValueTask OnBanCreated(BanCreatedEventArgs e)
    {
        await _mediator.Send(new UserBanned
        {
            GuildId = e.GuildId,
            UserId = e.UserId
        });
    }

    protected override async ValueTask OnBanDeleted(BanDeletedEventArgs e)
    {
        await _mediator.Send(new UserUnbanned
        {
            GuildId = e.GuildId,
            UserId = e.UserId
        });
    }

    protected override ValueTask OnVoiceServerUpdated(VoiceServerUpdatedEventArgs e)
    {
        return base.OnVoiceServerUpdated(e);
    }
    
    protected override async ValueTask OnVoiceStateUpdated(VoiceStateUpdatedEventArgs e)
    {
        await _mediator.Send(new VoiceStateUpdate
        {
            GuildId = e.GuildId,
            ChannelId = e.NewVoiceState.ChannelId,
            SessionId = e.NewVoiceState.SessionId,
            UserId = e.MemberId
        });
    }
    
    protected override async ValueTask OnReactionAdded(ReactionAddedEventArgs e)
    {
        if(e.GuildId.HasValue is false) return;
        await _mediator.Send(new ReactionAdd
        {
            GuildId = e.GuildId.Value,
            ChannelId = e.ChannelId,
            MessageId = e.MessageId,
            UserId = e.UserId,
            Emoji = e.Emoji.GetReactionFormat()
        });
    }
    
    protected override async ValueTask OnReactionRemoved(ReactionRemovedEventArgs e)
    {
        if(e.GuildId.HasValue is false) return;
        await _mediator.Send(new ReactionRemove
        {
            GuildId = e.GuildId.Value,
            ChannelId = e.ChannelId,
            MessageId = e.MessageId,
            UserId = e.UserId,
            Emoji = e.Emoji.GetReactionFormat()
        });
    }
    
    protected override async ValueTask OnReactionsCleared(ReactionsClearedEventArgs e)
    {
        if(e.GuildId.HasValue is false) return;
        await _mediator.Send(new ReactionCleared
        {
            GuildId = e.GuildId.Value,
            ChannelId = e.ChannelId,
            MessageId = e.MessageId
        });
    }
    
    private static HashSet<ulong> ConvertRoles(IEnumerable<Snowflake> roles)
    {
        var toReturn = new HashSet<ulong>();
        foreach (var role in roles) 
            toReturn.Add(role);
        return toReturn;
    }
}