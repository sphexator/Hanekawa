using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Decorator;
using Hanekawa.Entities;
using Microsoft.Extensions.Configuration;

namespace Hanekawa.Application.Handlers.Services.Levels;

public class MessageReceivedExperienceHandler: INotificationHandler<MessageReceived>
{
    private readonly IConfiguration _configuration;
    private readonly ILevelService _levelService;
    private readonly IModuleService _moduleService;

    public MessageReceivedExperienceHandler(ILevelService levelService, IConfiguration configuration,
        IModuleService moduleService)
    {
        _levelService = levelService;
        _configuration = configuration;
        _moduleService = moduleService;
    }

    public async Task HandleAsync(MessageReceived notification, CancellationToken cancellationToken)
    {
        if (!await _moduleService.IsEnabledAsync(notification.GuildId, ModuleName.Level, cancellationToken))
            return;

        var lower = _configuration.GetValue("expLower", 1);
        var upper = _configuration.GetValue("expUpper", 5);
        if (upper <= lower)
            upper = lower + 1;

        await _levelService.AddExperienceAsync(notification.Member,
            Random.Shared.Next(lower, upper));
    }
}