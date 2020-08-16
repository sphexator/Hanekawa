namespace Hanekawa.HungerGames.Entity
{
    public class Participant
    {
        public Participant(ulong id)
        {
            Id = id;
        }

        public ulong Id { get; set; }
        public string Name { get; set; } = "Test Unit";
        public string Avatar { get; set; } = null;
        public bool Alive { get; set; } = true;
        public int Health { get; set; } = 100;
        public int Stamina { get; set; } = 100;
        public int Hunger { get; set; } = 0;
        public int Thirst { get; set; } = 0;
        public int Tiredness { get; set; } = 0;
        public bool Bleeding { get; set; } = false;
        public object Inventory { get; set; }
        public ActionType LatestMove { get; set; } = ActionType.Idle;
    }
}