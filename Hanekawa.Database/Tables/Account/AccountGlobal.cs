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
        public int UserColor { get; set; } = Convert.ToInt32(Color.Purple.RawValue);
    }
}