using Discord.WebSocket;
using Hanekawa.Database.Tables.BotGame;
using Hanekawa.Extensions;

namespace Hanekawa.Models
{
    public class ShipGameUser
    {
        public ShipGameUser(GameEnemy enemy, GameClass gameClass)
        {
            PlayerOne = enemy.Name;
            PlayerOneId = (ulong)enemy.Id;
            IsNpc = true;
            Class = Class;
        }

        public ShipGameUser(SocketGuildUser userOne, GameClass gameClass, int? bet)
        {
            PlayerOne = userOne.GetName();
            PlayerOneId = userOne.Id;
            IsNpc = false;
            Bet = null;
            Class = gameClass;
            Bet = bet;
        }

        public string PlayerOne { get; set; }
        public ulong PlayerOneId { get; set; }
        public int Health { get; set; }
        public int DamageTaken { get;set; }
        public GameClass Class { get; set; }
        public bool IsNpc { get; set; }
        public int? Bet { get; set; }
    }
}