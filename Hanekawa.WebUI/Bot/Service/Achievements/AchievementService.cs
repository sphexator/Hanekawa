using System;
using Hanekawa.Entities;

namespace Hanekawa.WebUI.Bot.Service.Achievements
{
    public class AchievementService : INService
    {
        private readonly IServiceProvider _provider;

        public AchievementService(IServiceProvider provider) => _provider = provider;
    }
}
