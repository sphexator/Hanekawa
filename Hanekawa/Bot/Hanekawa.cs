using System;
using Disqord;
using Disqord.Bot;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qmmands;

namespace Hanekawa.Bot
{
    public class Hanekawa : DiscordBot
    {
        public Hanekawa(IOptions<DiscordBotConfiguration> options, ILogger<DiscordBot> logger, IPrefixProvider prefixes, 
            ICommandQueue queue, CommandService commands, IServiceProvider services, DiscordClient client) 
            : base(options, logger, prefixes, queue, commands, services, client)
        {
            
        }
    }
}