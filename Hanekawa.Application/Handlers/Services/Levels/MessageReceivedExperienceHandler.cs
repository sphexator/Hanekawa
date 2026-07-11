using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Decorator;
using Microsoft.Extensions.Configuration;

namespace Hanekawa.Application.Handlers.Services.Levels;

public class MessageReceivedExperienceHandler: INotificationHandler<MessageReceived>
{
    private readonly IConfiguration _configuration;
    private readonly ILevelService _levelService;

    public MessageReceivedExperienceHandler(ILevelService levelService, IConfiguration configuration)
    {
        _levelService = levelService;
        _configuration = configuration;
    }

    public async Task HandleAsync(MessageReceived notification, CancellationToken cancellationToken)
    {
        await _levelService.AddExperienceAsync(notification.Member,
            Random.Shared.Next(Convert.ToInt32(_configuration["expLower"]),
                Convert.ToInt32(_configuration["expUpper"])));
    }
}