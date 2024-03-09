using MediatR;

namespace Hanekawa.Application.Interfaces;

public interface IMetric : IRequest { ulong GuildId { get; init; } }
public interface IMetric<out T> : IRequest<T> { ulong GuildId { get; init; } }

public interface ISqs<out T> : IMetric<T>;
public interface ISqs : IMetric;