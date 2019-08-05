using System;
using Discord;

namespace Hanekawa.Database.Tables.Account
{
    public class AccountGlobal
    {
        public ulong UserId { get; set; }
        public int Level { get; set; } = 1;
        public int Exp { get; set; } = 0;
        public int TotalExp { get; set; } = 0;
        public int Rep { get; set; } = 0;
        public int Credit { get; set; } = 0;
        public int StarReceive { get; set; } = 0;
        public int StarGive { get; set; } = 0;
        public uint UserColor { get; set; } = Color.Purple.RawValue;
    }
}