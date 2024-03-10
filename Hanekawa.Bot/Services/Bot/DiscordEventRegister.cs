using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Hanekawa.Application.Contracts.Discord.Services;
using MediatR;

namespace Hanekawa.Bot.Services.Bot;

public class DiscordEventRegister(IServiceProvider service) : DiscordBotService
{
    protected override async ValueTask OnMemberJoined(MemberJoinedEventArgs e) =>
        await service.GetRequiredService<IMediator>()
            .Send(new UserJoin(e.GuildId, e.MemberId, e.Member.Name,
                e.Member.GetGuildAvatarUrl(), e.Member.CreatedAt())).ConfigureAwait(false);

    protected override async ValueTask OnMemberLeft(MemberLeftEventArgs e)
        => await service.GetRequiredService<IMediator>().Send(new UserLeave(e.GuildId, e.MemberId)).ConfigureAwait(false);

    protected override async ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
    {
        if (e.GuildId is null || e.Member is null) return;
        await service.GetRequiredService<IMediator>()
            .Send(new MessageReceived(e.GuildId.Value, e.ChannelId, new()
            {
                Guild = new() { Id = e.GuildId.Value },
                Id = e.Member.Id,
                RoleIds = ConvertRoles(e.Member.RoleIds),
                Nickname = e.Member.Nick,
                IsBot = e.Member.IsBot,
                Username = e.Member.Name,
                AvatarUrl = e.Member.GetAvatarUrl(),
                VoiceSessionId = e.Member.GetVoiceState()?.SessionId
            }, e.MessageId, e.Message.Content, e.Message.CreatedAt())).ConfigureAwait(false);
    }

    protected override async ValueTask OnMessageDeleted(MessageDeletedEventArgs e)
    {
        if (e.GuildId.HasValue is false || e.Message is null) return;
        await service.GetRequiredService<IMediator>()
            .Send(new MessageDeleted(e.GuildId.Value, e.ChannelId, e.Message.Author.Id,
                e.MessageId, e.Message.Content)).ConfigureAwait(false);
    }

    protected override async ValueTask OnMessagesDeleted(MessagesDeletedEventArgs e)
        => await service.GetRequiredService<IMediator>()
            .Send(new MessagesDeleted(e.GuildId, e.ChannelId,
                e.Messages.Select(x => x.Key.RawValue).ToArray(),
                e.MessageIds.Select(x => x.RawValue).ToArray(),
                e.Messages.Select(x => x.Value.Content).ToArray()))
            .ConfigureAwait(false);

    protected override async ValueTask OnBanCreated(BanCreatedEventArgs e)
        => await service.GetRequiredService<IMediator>()
            .Send(new UserBanned(new()
            {
                Guild = new() { Id = e.GuildId },
                Id = e.UserId,
                Username = e.User.Name,
                IsBot = e.User.IsBot,
                AvatarUrl = e.User.GetAvatarUrl()
            })).ConfigureAwait(false);

    protected override async ValueTask OnBanDeleted(BanDeletedEventArgs e)
        => await service.GetRequiredService<IMediator>()
            .Send(new UserUnbanned(new()
            {
                Guild = new() { Id = e.GuildId },
                Id = e.UserId,
                Username = e.User.Name,
                IsBot = e.User.IsBot,
                AvatarUrl = e.User.GetAvatarUrl()
            })).ConfigureAwait(false);

    protected override ValueTask OnVoiceServerUpdated(VoiceServerUpdatedEventArgs e) => base.OnVoiceServerUpdated(e);

    protected override async ValueTask OnVoiceStateUpdated(VoiceStateUpdatedEventArgs e)
    {
        await service.GetRequiredService<IMediator>()
            .Send(new VoiceStateUpdate(e.GuildId, e.MemberId, e.NewVoiceState.ChannelId,
                e.NewVoiceState.SessionId)).ConfigureAwait(false);
    }

    protected override async ValueTask OnReactionAdded(ReactionAddedEventArgs e)
    {
        if (e.GuildId.HasValue is false) return;
        await service.GetRequiredService<IMediator>()
            .Send(new ReactionAdd(e.GuildId.Value, e.ChannelId,
                e.MessageId, e.UserId, e.Emoji.GetReactionFormat())).ConfigureAwait(false);
    }

    protected override async ValueTask OnReactionRemoved(ReactionRemovedEventArgs e)
    {
        if (e.GuildId.HasValue is false) return;
        await service.GetRequiredService<IMediator>()
            .Send(new ReactionRemove(e.GuildId.Value, e.ChannelId, e.MessageId, e.UserId,
                e.Emoji.GetReactionFormat())).ConfigureAwait(false);
    }

    protected override async ValueTask OnReactionsCleared(ReactionsClearedEventArgs e)
    {
        if (e.GuildId.HasValue is false) return;
        await service.GetRequiredService<IMediator>()
            .Send(new ReactionCleared(e.GuildId.Value, e.ChannelId, e.MessageId))
            .ConfigureAwait(false);
    }

    private static HashSet<ulong> ConvertRoles(IEnumerable<Snowflake> roles)
    {
        var toReturn = new HashSet<ulong>();
        foreach (var role in roles)
            toReturn.Add(role);
        return toReturn;
    }
}