using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Application.Services;
using Hanekawa.Decorator;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Handlers.Services;

/// <summary>
/// Entry point of the message pipeline: applies the shared 1-minute rate limit.
/// Messages that pass are re-published as <see cref="MessageRateLimitPassed"/> so that
/// all downstream features (experience, weekly activity) share the same cooldown.
/// </summary>
public class MessageRateLimitHandler(IMessageRateLimiter rateLimiter, IEventPublisher publisher,
    ILogger<MessageRateLimitHandler> logger) : INotificationHandler<MessageReceived>
{
    public async Task HandleAsync(MessageReceived notification, CancellationToken cancellationToken)
    {
        if (!await rateLimiter.TryPassAsync(notification.GuildId, notification.Member.Id, cancellationToken))
        {
            logger.LogDebug("Message {Message} from user {User} in guild {Guild} dropped by rate limit",
                notification.MessageId, notification.Member.Id, notification.GuildId);
            return;
        }

        await publisher.PublishAsync(new MessageRateLimitPassed(notification.GuildId, notification.ChannelId,
            notification.Member, notification.MessageId, notification.Message, notification.CreatedAt),
            cancellationToken);
    }
}
