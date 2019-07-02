using Discord.WebSocket;
using Hanekawa.Database.Tables.BotGame;
using Hanekawa.Extensions;

namespace Hanekawa.Models
{
    public class ShipGameUser
    {
        public ShipGameUser(GameEnemy enemy, int level, GameClass gameClass)
        {
            Name = enemy.Name;
            Enemy = enemy;
            Id = (ulong)enemy.Id;
            IsNpc = true;
            Class = Class;
            Level = level;
        }

        public ShipGameUser(SocketGuildUser userOne, int level, GameClass gameClass)
        {
            Name = userOne.GetName();
            Id = userOne.Id;
            IsNpc = false;
            Bet = null;
            Class = gameClass;
            Level = level;
        }

        public string Name { get; set; }
        public ulong Id { get; set; }
        public int Level { get; set; }
        public int Health { get; set; }
        public int DamageTaken { get;set; }
        public GameClass Class { get; set; }
        public GameEnemy Enemy { get; set; }
        public bool IsNpc { get; set; }
        public int? Bet { get; set; }
    }
}