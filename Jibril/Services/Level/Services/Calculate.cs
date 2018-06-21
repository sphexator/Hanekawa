using Discord.WebSocket;
using System;

namespace Jibril.Services.Level.Services
{
    public abstract class Calculate
    {
        public uint GetNextLevelRequirement(uint currentLevel)
        {
            var calc = 3 * currentLevel * currentLevel + 150;
            return calc;
        }

        public uint GetMessageExp(SocketMessage msg)
        {
            var rand = new Random();
            var xp = rand.Next(10, 20);
            if (msg.Channel.Id.Equals(339383206669320192) || msg.Channel.Id.Equals(346429281314013184)) return Convert.ToUInt32(xp / 5);
            return Convert.ToUInt32(xp);
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

        // Voice Experience credit calculations = VECC
        /*
        public static void VECC(IUser user, DateTime vcTimer)
        {
            var calculateXp = GetVoiceExp(vcTimer) * 1;
            var calculateCredit = GetVoiceCredit(vcTimer);

            if (calculateXp > 0)
                LevelDatabase.AddExperience(user, calculateXp, calculateCredit);
        }
        */
    }
}