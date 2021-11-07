using System;

namespace Hanekawa.Entities.Account
{
    public class Account
    {
        public Account() { }
        public Account(ulong guildId, ulong userId)
        {
            GuildId = guildId;
            UserId = userId;
        }

        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }

        public bool Active { get; set; } = true;

        // Economy
        public int Credit { get; set; } = 0;
        public int CreditSpecial { get; set; } = 0;
        public DateTimeOffset DailyCredit { get; set; } = DateTimeOffset.UtcNow;

        // Level
        public int Level { get; set; } = 1;
        public int Exp { get; set; } = 0;
        public int TotalExp { get; set; } = 0;
        public int Decay { get; set; } = 0;

        public DateTimeOffset VoiceExpTime { get; set; } = DateTimeOffset.UtcNow;

        // Class Profile Role
        public int Class { get; set; } = 1;
        public string ProfilePic { get; set; } = null;
        public int Rep { get; set; } = 0;
        public DateTimeOffset RepCooldown { get; set; } = DateTimeOffset.UtcNow;

        public int GameKillAmount { get; set; } = 0;
        public int GamePvPAmount { get; set; } = 0;
        // Stats
        public DateTimeOffset? FirstMessage { get; set; } = null;
        public DateTimeOffset LastMessage { get; set; } = DateTimeOffset.UtcNow;
        public TimeSpan StatVoiceTime { get; set; } = TimeSpan.Zero;
        public int Sessions { get; set; } = 0;
        public long StatMessages { get; set; } = 0;
        public long DropClaims { get; set; } = 0;
        
        // Board
        public int StarGiven { get; set; } = 0;
        public int StarReceived { get; set; } = 0;

        // Misc
        public DateTimeOffset ChannelVoiceTime { get; set; } = DateTimeOffset.UtcNow;
        public string VoiceSessionId { get; set; } = null;
        
        // Mvp
        public int MvpCount { get; set; } = 0;
        public bool MvpOptOut { get; set; } = false;
        
        public void AddExp(int exp)
        {
            var expToNextLevel = ExpToNextLevel();
            if (Exp + exp >= expToNextLevel) LevelUp(exp, expToNextLevel);
            else Exp += exp;
            TotalExp += exp;
            Decay = 0;
        }
        
        public void AddCredit(int credit) => Credit += credit;
        public void AddSpecialCredit(int credit) => CreditSpecial += credit;

        private void LevelUp(int exp, int expToNextLevel)
        {
            Level++;
            Exp = (Exp + exp) - expToNextLevel;
        }

        public void ForceLevelUp()
        {
            TotalExp += ExpToNextLevel() - Exp;
            Exp = 0;
            Level++;
        }
        
        public int ExpToNextLevel() => 3 * Level * Level + 150;
    }
}