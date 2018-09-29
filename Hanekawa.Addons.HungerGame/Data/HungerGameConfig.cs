using System;

namespace Hanekawa.Addons.HungerGame.Data
{
    public class HungerGameConfig
    {
        public ulong GuildId { get; set; }
        public ulong MsgId { get; set; }
        public bool Live { get; set; }
        public int Round { get; set; }
        public DateTime SignupDuration { get; set; }
        public bool SignUpStage { get; set; }
    }
}