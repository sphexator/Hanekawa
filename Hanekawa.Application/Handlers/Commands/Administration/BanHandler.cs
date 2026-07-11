using Hanekawa.Application.Contracts.Discord.Common;
using Hanekawa.Application.Extensions;
using Hanekawa.Decorator;

namespace Hanekawa.Application.Handlers.Commands.Administration;

public class BanHandler(IServiceProvider services) : IRequestHandler<Ban>
{ 
    public Task HandleAsync(Ban request, CancellationToken cancellationToken)
    { 
	    var bot = request.Source.GetClient(services); 
        return bot.BanAsync(request.GuildId, 
            request.UserId,
				    request.Days, 
            request.Reason + $" %{request.ModeratorId}%"); 
    }
}