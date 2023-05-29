using MediatR;

namespace Hanekawa.Application.Interfaces;

public interface IMetric
{
    ulong GuildId { get; init; }
}

public interface ISqs<out T> : IRequest<T>, IMetric
{}
public interface ISqs : IRequest, IMetric
{}