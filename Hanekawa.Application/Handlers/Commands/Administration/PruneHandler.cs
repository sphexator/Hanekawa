using Hanekawa.Application.Contracts.Discord.Common;
using Hanekawa.Application.Extensions;
using MediatR;

namespace Hanekawa.Application.Handlers.Commands.Administration;

public class PruneHandler : IRequestHandler<Prune>
{
	private readonly IServiceProvider _services;
	
	public PruneHandler(IServiceProvider services)
		=> _services = services;
	
	public Task Handle(Prune request, CancellationToken cancellationToken)
	{
		var bot = request.Source.GetClient(_services);
		return bot.PruneMessagesAsync(request.GuildId,
				request.UserId,
				[]);
	}
}