using Hanekawa.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Hanekawa.Application.Handlers.Levels;

public class MessageReceivedExperienceHandler: IRequestHandler<Contracts.Discord.MessageReceived, bool>
{
    private readonly IConfiguration _configuration;
    private readonly ILevelService _levelService;
    
    public MessageReceivedExperienceHandler(ILevelService levelService, IConfiguration configuration)
    {
        _levelService = levelService;
        _configuration = configuration;
    }
    
    public async Task<bool> Handle(Contracts.Discord.MessageReceived request, CancellationToken cancellationToken)
    {
        await _levelService.AddExperience(request.Member,
            Random.Shared.Next(Convert.ToInt32(_configuration["expLower"]),
                Convert.ToInt32(_configuration["expUpper"])));
        return true;
    }
}