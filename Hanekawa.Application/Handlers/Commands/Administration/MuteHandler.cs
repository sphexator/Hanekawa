using Hanekawa.Application.Contracts.Discord.Common;
using Hanekawa.Application.Extensions;
using MediatR;

namespace Hanekawa.Application.Handlers.Commands.Administration;

public class MuteHandler : IRequestHandler<Mute>
{
	private readonly IServiceProvider _services;
	public MuteHandler(IServiceProvider services)
		=> _services = services;
	
	public Task Handle(Mute request, CancellationToken cancellationToken)
	{
		var bot = request.Source.GetClient(_services);
		return bot.MuteAsync(request.GuildId,
				request.UserId,
				request.Reason + $" %{request.ModeratorId}%",
				request.Duration);
	}
}