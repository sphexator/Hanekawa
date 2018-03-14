using System;

namespace Jibril.Services.INC.Data
{
    public class Config
    {
        public ulong GuildId { get; set; }
        public bool Live { get; set; }
        public int Round { get; set; }
        public DateTime SignupDuration { get; set; }
        public bool SignUpStage { get; set; }
    }
}