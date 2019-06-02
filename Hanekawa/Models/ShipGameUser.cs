using Discord.WebSocket;
using Hanekawa.Database.Tables.BotGame;
using Hanekawa.Extensions;

namespace Hanekawa.Models
{
    public class ShipGameUser
    {
        public ShipGameUser(GameEnemy enemy)
        {
            PlayerOne = enemy.Name;
            PlayerOneId = (ulong)enemy.Id;
            IsNpc = true;
        }

        public ShipGameUser(SocketGuildUser userOne)
        {
            PlayerOne = userOne.GetName();
            PlayerOneId = userOne.Id;
            IsNpc = false;
            Bet = null;
        }

        public ShipGameUser(SocketGuildUser userOne, int bet)
        {
            PlayerOne = userOne.GetName();
            PlayerOneId = userOne.Id;
            IsNpc = false;
            Bet = bet;
        }

        public string PlayerOne { get; set; }
        public ulong PlayerOneId { get; set; }
        public int Health { get; set; }
        public bool IsNpc { get; set; }
        public int? Bet { get; set; }
    }
}
