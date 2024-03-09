using Hanekawa.Application.Contracts.Discord.Common;
using Hanekawa.Application.Extensions;
using MediatR;

namespace Hanekawa.Application.Handlers.Commands.Administration;

public class KickHandler : IRequestHandler<Kick>
{
	private readonly IServiceProvider _service;

	public KickHandler(IServiceProvider service)
	{
		_service = service;
	}

	public Task Handle(Kick request, CancellationToken cancellationToken)
	{
		var bot = request.Source.GetClient(_service);
		return bot.KickAsync(request.GuildId, 
				request.UserId, 
				request.Reason + $" %{request.ModeratorId}%");
	}
}