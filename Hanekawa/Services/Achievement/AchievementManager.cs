using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Hanekawa.Addons.Database.Tables.Account;
using Hanekawa.Services.Drop;
using Hanekawa.Services.Level;

namespace Hanekawa.Services.Achievement
{
    public class AchievementManager
    {
        private readonly DiscordSocketClient _client;
        private readonly DropService _dropService;
        private readonly LevelingService _levelingService;

        public AchievementManager(DiscordSocketClient client, DropService dropService, LevelingService levelingService)
        {
            _client = client;
            _dropService = dropService;
            _levelingService = levelingService;

            _client.MessageReceived += MessageReceived;
            _dropService.DropClaimed += DropClaimed;
            _levelingService.Level += LevelAchievement;
            _levelingService.InVoice += InVoiceAchievement;
        }

        private Task InVoiceAchievement(SocketGuildUser user, TimeSpan time)
        {
            var _ = Task.Run(async () => { });
            return Task.CompletedTask;
        }

        private Task LevelAchievement(SocketGuildUser user, Account userData)
        {
            var _ = Task.Run(async () => { });
            return Task.CompletedTask;
        }

        private Task DropClaimed(SocketGuildUser user)
        {
            var _ = Task.Run(async () => { });
            return Task.CompletedTask;
        }

        private Task MessageReceived(SocketMessage user)
        {
            var _ = Task.Run(async () => { });
            return Task.CompletedTask;
        }
    }
}