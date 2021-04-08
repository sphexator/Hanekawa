using System;
using NLog;

namespace Hanekawa.Bot.Service.Achievements
{
    public partial class AchievementService : INService
    {
        // TODO: Redesign all of achievements
        private readonly IServiceProvider _provider;
        private readonly Hanekawa _bot;
        private readonly Logger _logger;

        public AchievementService(IServiceProvider provider, Hanekawa bot, Logger logger)
        {
            _provider = provider;
            _bot = bot;
            _logger = logger;
        }

        private const int Special = 1;
        private const int Voice = 2;
        private const int Level = 3;
        private const int Drop = 4;
        private const int PvP = 5;
        private const int PvE = 6;
        private const int Fun = 7;
    }
}
