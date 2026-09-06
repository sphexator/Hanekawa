using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Decorator;
using Hanekawa.Entities;

namespace Hanekawa.Application.Handlers.Services;

public class StreamHandler : IRequestHandler<PresenceUpdated, Response<PresenceUpdated>>
{
	public Task<Response<PresenceUpdated>> HandleAsync(PresenceUpdated request, CancellationToken cancellationToken = default)
	{
		request.Deconstruct(out var user, out var oldPresence, out var newPresence);
		
		return new Task<Response<PresenceUpdated>>(() => new Response<PresenceUpdated>(request));
	}
}