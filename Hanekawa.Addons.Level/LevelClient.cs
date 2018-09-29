using System;

namespace Hanekawa.Addons.Level
{
    public class LevelClient
    {
        public int GetGlobalLevelRequirement(int level)
        {
            return 50 * level * level + 300;
        }

        public int GetServerLevelRequirement(int level)
        {
            return 3 * level * level + 150;
        }

        public int GetExperience(bool reducedExp = false)
        {
            var rand = new Random();
            var xp = rand.Next(10, 20);
            if (reducedExp)xp = xp / 2;
            return xp;
        }

        public int GetCredit()
        {
            return new Random().Next(1, 3);
        }

        public int GetVoiceExperience(DateTime time)
        {
            var now = DateTime.UtcNow;

            var diff = now - time;
            var hours = diff.Hours;
            var minutes = diff.Minutes;
            var totalTime = hours * 60 + minutes;
            return totalTime * 2;
        }

        public int GetVoiceCredit(DateTime time)
        {
            var now = DateTime.UtcNow;

            var diff = now - time;
            var hours = int.Parse(diff.Hours.ToString());
            var minutes = int.Parse(diff.Minutes.ToString());
            return hours * 60 + minutes;
        }
    }
}
