using Hanekawa.Application.Contracts.Discord.Common;
using Hanekawa.Application.Extensions;
using Hanekawa.Decorator;

namespace Hanekawa.Application.Handlers.Commands.Administration;

public class UnmuteHandler : IRequestHandler<Unmute>
{
	private readonly IServiceProvider _services;
	public UnmuteHandler(IServiceProvider services)
		=> _services = services;
	
	public Task HandleAsync(Unmute request, CancellationToken cancellationToken)
	{
		var bot = request.Source.GetClient(_services);
		return bot.UnmuteAsync(request.GuildId,
				request.UserId,
				request.Reason + $" %{request.ModeratorId}%");
	}
}