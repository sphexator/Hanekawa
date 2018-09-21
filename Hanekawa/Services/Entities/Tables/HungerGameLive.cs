namespace Hanekawa.Services.Entities.Tables
{
    public class HungerGameLive
    {
        public ulong UserId { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
        public uint Health { get; set; }
        public uint Stamina { get; set; }
        public uint Fatigue { get; set; }
        public uint Hunger { get; set; }
        public uint Thirst { get; set; }
        public uint Sleep { get; set; }
        public bool Bleeding { get; set; }
        public uint Food { get; set; }
        public uint Water { get; set; }
        public uint TotalWeapons { get; set; }
        public uint Pistol { get; set; }
        public uint Axe { get; set; }
        public uint Bow { get; set; }
    }
}
