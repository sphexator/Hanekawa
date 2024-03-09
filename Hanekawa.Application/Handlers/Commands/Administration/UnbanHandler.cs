using Hanekawa.Application.Contracts.Discord.Common;
using Hanekawa.Application.Extensions;
using MediatR;

namespace Hanekawa.Application.Handlers.Commands.Administration;

public class UnbanHandler : IRequestHandler<Unban>
{
	private readonly IServiceProvider _services;
	
	public UnbanHandler(IServiceProvider services)
		=> _services = services;

	public Task Handle(Unban request, CancellationToken cancellationToken)
	{
		var bot = request.Source.GetClient(_services);
		return bot.UnbanAsync(request.GuildId,
				request.UserId,
				request.Reason + $" %{request.ModeratorId}%");
	}
}