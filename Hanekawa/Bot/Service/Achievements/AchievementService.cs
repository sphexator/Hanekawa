using System;
using Hanekawa.Entities;
using NLog;

namespace Hanekawa.Bot.Service.Achievements
{
    public abstract partial class AchievementService : INService
    {
        private readonly IServiceProvider _provider;
        private readonly Logger _logger;

        protected AchievementService(IServiceProvider provider)
        {
            _provider = provider;
            _logger = LogManager.GetCurrentClassLogger();
        }
    }
}
