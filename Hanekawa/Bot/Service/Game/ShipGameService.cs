using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Rest;
using Hanekawa.Bot.Service.Achievements;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Account.ShipGame;
using Hanekawa.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Hanekawa.Bot.Service.Game
{
    public class ShipGameService : INService
    {
        private readonly Hanekawa _bot;
        private readonly IServiceProvider _provider;
        private readonly Logger _logger;
        private readonly Random _random;
        private readonly AchievementService _achievement;
        private int DefaultHealth { get; } = 10;
        private int DefaultDamage { get; } = 1;

        public ShipGameService(Hanekawa bot, IServiceProvider provider, Random random, AchievementService achievement)
        {
            _bot = bot;
            _provider = provider;
            _random = random;
            _achievement = achievement;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task<ShipUser> SearchAsync()
        {
            var chance = _random.Next(100);
            GameEnemy enemy;
            if (chance >= 40)
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var enemies = await db.GameEnemies.Where(x => !x.Elite && !x.Rare).ToListAsync();
                enemy = enemies[_random.Next(enemies.Count)];
            }
            else return null;
            return await InitializeBattleAsync();
        }
        
        public async Task<ShipUser> InitializeBattleAsync(ShipGame game)
        {
            return await BattleAsync(game);
        }
        
        public async Task<ShipUser> BattleAsync(ShipGame game)
        {
            var alive = true;
            var log = new LinkedList<string>();
            ShipUser winner;
            while (alive)
            {
                Round();
                alive = false;
            }

            if (game.Type == ShipGameType.PvE && !winner.IsNpc) _ = _achievement.PveKill();
            else _ = _achievement.PvpKill();
            return winner;
        }

        public void Round()
        {
            
        }
        
        private int CalculateDamage(int damage, GameClass attackerClass, GameClass enemyClass, EnemyType type)
        {
            var avoidance = _random.Next(100);
            var criticalChance = _random.Next(100);
            if (type == EnemyType.Player)
                if (avoidance <= enemyClass.ChanceAvoid)
                    return 0;
            if (criticalChance <= attackerClass.ChanceCrit)
                damage = Convert.ToInt32(damage * attackerClass.ModifierCriticalChance);
            var lowDmg = damage / 2;
            if (lowDmg <= 0) lowDmg = 5;
            var highDmg = damage * 2;
            if (lowDmg >= highDmg) highDmg = lowDmg + 10;
            return _random.Next(lowDmg, highDmg);
        }
        private int Damage(int level) => DefaultDamage * level;
        private int Damage(int level, GameEnemy enemy) => (DefaultDamage + enemy.Damage) * level;
        private int GetHealth(int level, GameClass ass) =>
            Convert.ToInt32(Math.Round(DefaultHealth * level * ass.ModifierHealth));

        private int GetHealth(int level, GameEnemy enemyData, GameClass enemyClass) =>
            Convert.ToInt32(Math.Round((DefaultHealth + enemyData.Health) * level *
                                       enemyClass.ModifierHealth));
    }
}