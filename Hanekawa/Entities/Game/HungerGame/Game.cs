using System;
using System.Collections.Generic;

namespace Hanekawa.Entities.Game.HungerGame
{
    public class Game
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public ulong GuildId { get; set; }
        public GameStage Stage = GameStage.Signup;
        public DateTimeOffset Start { get; set; } = DateTimeOffset.UtcNow;
        public int Round { get; set; } = 0;
        public int MaxYPosition { get; set; } = 20;
        public int MaxXPosition { get; set; } = 20;

        public List<Participant> Participants { get; set; } = new ();
    }
}