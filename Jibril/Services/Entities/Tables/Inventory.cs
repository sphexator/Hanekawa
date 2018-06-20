namespace Jibril.Services.Entities.Tables
{
    public class Inventory
    {
        public ulong UserId { get; set; }
        public uint RepairKit { get; set; }
        public uint DamageBoost { get; set; }
        public uint Shield { get; set; }
        public uint CustomRole { get; set; }
        public uint Gift { get; set; }
    }
}