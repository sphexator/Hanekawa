namespace Hanekawa.Models
{
    public class ShipGame
    {
        public ShipGame(ShipGameUser playerOne, ShipGameUser playerTwo)
        {
            PlayerOne = playerOne;
            PlayerTwo = playerTwo;
        }

        public ShipGameUser PlayerOne { get; }
        public ShipGameUser PlayerTwo { get; }
    }
}