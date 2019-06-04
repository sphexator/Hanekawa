namespace Hanekawa.Models
{
    public class ShipGame
    {
        public ShipGame(ShipGameUser playerOne, ShipGameUser playerTwo, int? bet)
        {
            PlayerOne = playerOne;
            PlayerTwo = playerTwo;
            Bet = bet;
        }

        public ShipGameUser PlayerOne { get; }
        public ShipGameUser PlayerTwo { get; }
        public int? Bet { get; set; }
    }
}