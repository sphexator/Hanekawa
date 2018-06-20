namespace Jibril.Services.Entities.Tables
{
    public class GameEnemy
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public uint Health { get; set; }
        public uint Damage { get; set; }
        public string Class { get; set; }
        public uint ExpGain { get; set; }
        public uint CreditGain { get; set; }
    }
}
