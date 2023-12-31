using MediatR;

namespace Hanekawa.Application.Handlers.Services.Metrics;

public record CommandMetric(ulong GuildId, ulong UserId, string Command, DateTimeOffset Timestamp) : IRequest;

public class CommandMetricHandler : IRequestHandler<CommandMetric>
{
    public Task Handle(CommandMetric request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}