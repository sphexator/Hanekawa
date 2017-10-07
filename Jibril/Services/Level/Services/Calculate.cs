using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.Level.Services
{
    public class Calculate
    {
        public static int ReturnXP(SocketMessage msg)
        {
            Random rand = new Random();
            var defxp = rand.Next(10, 20);
            if(msg.Channel.Id == 339383206669320192 || msg.Channel.Id == 346429281314013184)
            {
                var xp = defxp / 5;
                return xp;
            }
            else
            {
                return defxp;
            }
        }

        public static int CalculateNextLevel(int currentLevel)
        {
            var calc = 3 * (currentLevel * currentLevel) + 150;
            return calc;
        }

        public static int ReturnCredit()
        {
            Random rand = new Random();
            int credit = rand.Next(1, 3);
            return credit;
        }

        //Voice Experience credit calculations = VECC
        public static void VECC(IUser user, DateTime vcTimer)
        {
            DateTime now = DateTime.Now;

            TimeSpan diff = now - vcTimer;
            Int32 hours = Int32.Parse(diff.Hours.ToString());
            Int32 minutes = Int32.Parse(diff.Minutes.ToString());
            Int32 totalTime = (hours * 60) + minutes;

            int CalculateXP = totalTime * 2;
            int CalculateCredit = totalTime;

            if (CalculateXP > 0)
            {
                LevelDatabase.AddExperience(user, CalculateXP, CalculateCredit);
                return;
            }
            else return;
        }
    }
}
