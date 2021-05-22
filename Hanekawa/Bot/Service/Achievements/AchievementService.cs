using System;
using Hanekawa.Entities;

namespace Hanekawa.Bot.Service.Achievements
{
    public partial class AchievementService : INService
    {
        private readonly IServiceProvider _provider;

        public AchievementService(IServiceProvider provider) => _provider = provider;
    }
}
