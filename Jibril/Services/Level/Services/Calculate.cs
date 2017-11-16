using System;
using Discord;
using Discord.WebSocket;

namespace Jibril.Services.Level.Services
{
    public class Calculate
    {
        public static int ReturnXP(SocketMessage msg)
        {
            var xp = CalculateExperience(msg);
            var returnExp = xp * 2;
            return returnExp;
        }

        public static int CalculateNextLevel(int currentLevel)
        {
            var calc = 3 * currentLevel * currentLevel + 150;
            return calc;
        }

        public static int ReturnCredit()
        {
            var credit = CalculateCredit();
            return credit;
        }

        //Voice Experience credit calculations = VECC
        public static void VECC(IUser user, DateTime vcTimer)
        {
            var calculateXp = CalculateVoiceExperience(vcTimer) * 2;
            var calculateCredit = CalculateVoiceCredit(vcTimer);

            if (calculateXp > 0)
                LevelDatabase.AddExperience(user, calculateXp, calculateCredit);
        }

        private static int CalculateExperience(SocketMessage msg)
        {
            var rand = new Random();
            var xp = rand.Next(10, 20);
            if (msg.Channel.Id == 339383206669320192 || msg.Channel.Id == 346429281314013184) return xp / 5;
            return xp;
        }

        private static int CalculateCredit()
        {
            var rand = new Random();
            var credit = rand.Next(1, 3);
            return credit;
        }

        private static int CalculateVoiceExperience(DateTime vcTimer)
        {
            var now = DateTime.Now;

            var diff = now - vcTimer;
            var hours = int.Parse(diff.Hours.ToString());
            var minutes = int.Parse(diff.Minutes.ToString());
            var totalTime = hours * 60 + minutes;

            var calculateXp = totalTime * 2;
            return calculateXp;
        }

        private static int CalculateVoiceCredit(DateTime vcTimer)
        {
            var now = DateTime.Now;

            var diff = now - vcTimer;
            var hours = int.Parse(diff.Hours.ToString());
            var minutes = int.Parse(diff.Minutes.ToString());
            var totalTime = hours * 60 + minutes;

            return totalTime;
        }
    }
}