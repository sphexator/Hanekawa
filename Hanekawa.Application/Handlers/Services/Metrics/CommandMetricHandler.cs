using Hanekawa.Decorator;

namespace Hanekawa.Application.Handlers.Services.Metrics;

public record CommandMetric(ulong GuildId, ulong UserId, string Command, DateTimeOffset Timestamp) : IRequest;

public class CommandMetricHandler : IRequestHandler<CommandMetric>
{
    public Task HandleAsync(CommandMetric request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}