namespace Hanekawa.Services.Entities.Tables
{
    public class GameEnemy
    {
        public uint Id { get; set; }
        public string Name { get; set; }

        public bool Elite { get; set; }
        public bool Rare { get; set; }

        public string ImageUrl { get; set; }

        public uint Health { get; set; }
        public uint Damage { get; set; }

        public int ClassId { get; set; }

        public uint ExpGain { get; set; }
        public uint CreditGain { get; set; }
    }
}