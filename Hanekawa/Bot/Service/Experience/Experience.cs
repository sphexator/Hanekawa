using System;
using System.Threading.Tasks;
using Disqord.Gateway;
using Hanekawa.Bot.Service.Cache;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using Quartz;

namespace Hanekawa.Bot.Service
{
    public partial class Experience : INService, IJob
    {
        private readonly CacheService _cache;
        private readonly Logger _logger;
        private readonly Hanekawa _bot;

        public Experience(CacheService cache, Hanekawa bot)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _cache = cache;
            _bot = bot;
        }
        
        public async Task VoiceExperienceAsync(VoiceStateUpdatedEventArgs e)
        {
            
        }

        public async Task ServerExperienceAsync(MessageReceivedEventArgs e)
        {
            
        }

        public async Task GlobalExperienceAsync(MessageReceivedEventArgs e)
        {
            if (e.Member == null) return;
            if (_cache.GlobalCooldown.TryGetValue(e.Member.Id, out _)) return;
            _cache.GlobalCooldown.Set(e.Member.Id, 0, TimeSpan.FromMinutes(1));
            
        }

        public Task Execute(IJobExecutionContext context)
        {
            
            throw new System.NotImplementedException();
        }
    }
}