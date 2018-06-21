namespace Jibril.Services.Entities.Tables
{
    public class LevelReward
    {
        public uint Level { get; set; }
        public string Name { get; set; }
        public ulong Role { get; set; }
        public bool Stackable { get; set; }
    }
}
