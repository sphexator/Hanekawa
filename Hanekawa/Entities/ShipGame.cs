using Disqord.Gateway;

namespace Hanekawa.Entities
{
    public record ShipGame
    {
        public ShipGame() { }
        public ShipGame(ShipUser playerOne, ShipUser playerTwo, int? bet, int? exp, int? credit, ShipGameType type, CachedTextChannel channel)
        {
            PlayerOne = playerOne;
            PlayerTwo = playerTwo;
            Bet = bet;
            Type = type;
            Channel = channel;
        }

        public ShipUser PlayerOne { get; set; }
        public ShipUser PlayerTwo { get; set; }
        public int? Exp { get; set; }
        public int? Credit { get; set; }
        public int? Bet { get; set; }
        public ShipGameType Type { get; set; }
        public CachedTextChannel Channel { get; set; }
    }
}