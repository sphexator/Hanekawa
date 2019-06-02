using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Extensions;

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
