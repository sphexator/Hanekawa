namespace Jibril.Services.Entities.Tables
{
    public class LevelReward
    {
        public ulong GuildId { get; set; }
        public uint Level { get; set; }
        public ulong Role { get; set; }
        public bool Stackable { get; set; }
    }
}
