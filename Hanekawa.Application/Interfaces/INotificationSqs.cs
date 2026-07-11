using Hanekawa.Decorator;

namespace Hanekawa.Application.Interfaces;

public interface IMetric { ulong GuildId { get; init; } }
public interface INotificationSqs : IMetric, INotification;

public interface IMessageSqs : IMetric, IRequest;
public interface IMessageSqs<out T> : IMetric, IRequest<T>;