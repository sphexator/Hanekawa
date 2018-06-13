using Discord.WebSocket;
using System;

namespace Jibril.Services.Level.Services
{
    public static class Calculate
    {
        // Calculate exp
        public static int CalculateNextLevel(int currentLevel)
        {
            var calc = 3 * currentLevel * currentLevel + 150;
            return calc;
        }

        // Give reward
        public static int MessageExperience(SocketMessage msg)
        {
            var def = CalculateExperience(msg);
            var returnExp = def * 1; //Modifier to exp gain
            return returnExp;
        }

        public static uint MessageCredit()
        {
            var def = CalculateCredit();
            var credit = def * 1; //Modifier to credit gain
            return Convert.ToUInt32(credit);
        }

        // Voice Experience credit calculations = VECC
        /*
        public static void VECC(IUser user, DateTime vcTimer)
        {
            var calculateXp = CalculateVoiceExperience(vcTimer) * 1;
            var calculateCredit = CalculateVoiceCredit(vcTimer);

            if (calculateXp > 0)
                LevelDatabase.AddExperience(user, calculateXp, calculateCredit);
        }
        */

        private static int CalculateExperience(SocketMessage msg)
        {
            var rand = new Random();
            var xp = rand.Next(10, 20);
            if (msg.Channel.Id.Equals(339383206669320192) || msg.Channel.Id.Equals(346429281314013184)) return xp / 5;
            return xp;
        }

        // Default Calculations of experience
        private static int CalculateCredit()
        {
            var rand = new Random();
            var credit = rand.Next(1, 3);
            return credit;
        }

        public static int CalculateVoiceExperience(DateTime vcTimer)
        {
            var now = DateTime.UtcNow;

            var diff = now - vcTimer;
            var hours = int.Parse(diff.Hours.ToString());
            var minutes = int.Parse(diff.Minutes.ToString());
            var totalTime = hours * 60 + minutes;
            var calculateXp = totalTime * 2;

            return calculateXp;
        }

        public static uint CalculateVoiceCredit(DateTime vcTimer)
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