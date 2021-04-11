namespace Hanekawa.Entities
{
    public record ShipGame
    {
        public ShipGame(ShipUser playerOne, ShipUser playerTwo, int? bet, ShipGameType type)
        {
            PlayerOne = playerOne;
            PlayerTwo = playerTwo;
            Bet = bet;
            Type = type;
        }

        public ShipUser PlayerOne { get; }
        public ShipUser PlayerTwo { get; }
        public int? Bet { get; set; }
        
        public ShipGameType Type { get; set; }
    }
}