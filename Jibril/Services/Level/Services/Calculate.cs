using System;
using Discord;
using Discord.WebSocket;

namespace Jibril.Services.Level.Services
{
    public class Calculate
    {
        public static int ReturnXP(SocketMessage msg)
        {
            var rand = new Random();
            var defxp = rand.Next(10, 20);
            if (msg.Channel.Id == 339383206669320192 || msg.Channel.Id == 346429281314013184)
            {
                var xp = defxp / 5;
                return xp;
            }
            return defxp;
        }

        public static int CalculateNextLevel(int currentLevel)
        {
            var calc = 3 * currentLevel * currentLevel + 150;
            return calc;
        }

        public static int ReturnCredit()
        {
            var rand = new Random();
            var credit = rand.Next(1, 3);
            return credit;
        }

        //Voice Experience credit calculations = VECC
        public static void VECC(IUser user, DateTime vcTimer)
        {
            var now = DateTime.Now;

            var diff = now - vcTimer;
            var hours = int.Parse(diff.Hours.ToString());
            var minutes = int.Parse(diff.Minutes.ToString());
            var totalTime = hours * 60 + minutes;

            var CalculateXP = totalTime * 2;
            var CalculateCredit = totalTime;

            if (CalculateXP > 0)
            {
                LevelDatabase.AddExperience(user, CalculateXP, CalculateCredit);
            }
        }
    }
}