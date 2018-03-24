﻿using System;
using Discord;
using Discord.WebSocket;

namespace Jibril.Services.Level.Services
{
    public class Calculate
    {
        // Calculate exp
        public static int CalculateNextLevel(int currentLevel)
        {
            var calc = 3 * currentLevel * currentLevel + 150;
            return calc;
        }

        // Give reward
        public static int ReturnXP(SocketMessage msg)
        {
            var def = CalculateExperience(msg);
            var returnExp = def * 2; //Modifier to exp gain
            return returnExp;
        }

        public static int ReturnCredit()
        {
            var def = CalculateCredit();
            var credit = def * 2; //Modifier to credit gain
            return credit;
        }

        // Voice Experience credit calculations = VECC
        public static void VECC(IUser user, DateTime vcTimer)
        {
            var calculateXp = CalculateVoiceExperience(vcTimer) * 1;
            var calculateCredit = CalculateVoiceCredit(vcTimer);

            if (calculateXp > 0)
                LevelDatabase.AddExperience(user, calculateXp, calculateCredit);
        }

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