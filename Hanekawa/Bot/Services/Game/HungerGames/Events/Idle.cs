namespace Hanekawa.Bot.Services.Game.HungerGames.Events
{
    public partial class HgEvent
    {
        private readonly string[] _idleStrings = {
            "Climbs a tree",
            "Looks at the sky, pondering about life",
            "Frozen in time",
            "Standing still, looking at a tree",
            "Wonders if its possible to do ninjutsu"
        };

        public string Idle() => _idleStrings[_random.Next(_idleStrings.Length)];
    }
}