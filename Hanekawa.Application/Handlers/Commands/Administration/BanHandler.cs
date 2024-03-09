using Hanekawa.Application.Contracts.Discord.Common;
using Hanekawa.Application.Extensions;
using MediatR;

namespace Hanekawa.Application.Handlers.Commands.Administration;

public class BanHandler : IRequestHandler<Ban>
{
	private readonly IServiceProvider _services;
	
	public BanHandler(IServiceProvider services)
		=> _services = services;

	public Task Handle(Ban request, CancellationToken cancellationToken)
	{
		var bot = request.Source.GetClient(_services);
		return bot.BanAsync(request.GuildId,
				request.UserId,
				request.Days,
				request.Reason + $" %{request.ModeratorId}%");
	}
}