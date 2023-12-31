using Hanekawa.Application.Contracts.Discord;
using Hanekawa.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Hanekawa.Application.Handlers.Services.Levels;

public class MessageReceivedExperienceHandler: IRequestHandler<MessageReceived, bool>
{
    private readonly IConfiguration _configuration;
    private readonly ILevelService _levelService;
    
    public MessageReceivedExperienceHandler(ILevelService levelService, IConfiguration configuration)
    {
        _levelService = levelService;
        _configuration = configuration;
    }
    
    public async Task<bool> Handle(MessageReceived request, CancellationToken cancellationToken)
    {
        await _levelService.AddExperienceAsync(request.Member,
            Random.Shared.Next(Convert.ToInt32(_configuration["expLower"]),
                Convert.ToInt32(_configuration["expUpper"])));
        return true;
    }
}