using System;
using Discord.WebSocket;

namespace Hanekawa.Services.Level.Util
{
    public class Calculate
    {
        public uint GetServerLevelRequirement(uint currentLevel)
        {
            return 3 * currentLevel * currentLevel + 150;
        }

        public uint GetGlobalLevelRequirement(uint currentLevel)
        {
            return 50 * currentLevel * currentLevel + 300;
        }

        public uint GetMessageExp(SocketMessage msg, bool reduced = false)
        {
            var rand = new Random();
            var xp = rand.Next(10, 20);
            return reduced ? Convert.ToUInt32(xp / 10) : Convert.ToUInt32(xp);
        }

        public uint GetMessageCredit()
        {
            var rand = new Random();
            var credit = rand.Next(1, 3);
            return Convert.ToUInt32(credit);
        }

        public uint GetVoiceExp(DateTime vcTimer)
        {
            var now = DateTime.UtcNow;

            var diff = now - vcTimer;
            var hours = int.Parse(diff.Hours.ToString());
            var minutes = int.Parse(diff.Minutes.ToString());
            var totalTime = hours * 60 + minutes;
            var calculateXp = totalTime * 2;

            return Convert.ToUInt32(calculateXp);
        }

        public uint GetVoiceCredit(DateTime vcTimer)
        {
            var now = DateTime.UtcNow;

            var diff = now - vcTimer;
            var hours = int.Parse(diff.Hours.ToString());
            var minutes = int.Parse(diff.Minutes.ToString());
            var totalTime = hours * 60 + minutes;

            return Convert.ToUInt32(totalTime);
        }
    }
}