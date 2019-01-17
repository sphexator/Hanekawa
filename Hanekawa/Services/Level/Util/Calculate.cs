using Hanekawa.Entities.Interfaces;
using System;

namespace Hanekawa.Services.Level.Util
{
    public class Calculate : IHanaService
    {
        private readonly Random _random;
        private int VoiceCalculate(int hours, int minutes) => hours * 60 * minutes;

        public Calculate(Random random) => _random = random;

        public int GetServerLevelRequirement(int currentLevel) => 3 * currentLevel * currentLevel + 150;

        public int GetGlobalLevelRequirement(int currentLevel) => 50 * currentLevel * currentLevel + 300;

        public int GetMessageExp(bool reduced = false)
        {
            var xp = _random.Next(10, 20);
            return reduced ? Convert.ToInt32(xp / 10) : Convert.ToInt32(xp);
        }

        public int GetMessageCredit() => _random.Next(1, 3);

        public int GetVoiceExp(DateTime vcTimer)
        {
            var diff = DateTime.UtcNow - vcTimer;
            return VoiceCalculate(diff.Hours, diff.Minutes) * 2;
        }

        public int GetVoiceCredit(DateTime vcTimer)
        {
            var diff = DateTime.UtcNow - vcTimer;
            return VoiceCalculate(diff.Hours, diff.Minutes);
        }
    }
}