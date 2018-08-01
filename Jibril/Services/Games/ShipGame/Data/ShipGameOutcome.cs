using Jibril.Services.Entities.Tables;

namespace Jibril.Services.Games.ShipGame.Data
{
    public class ShipGameOutcome
    {
        private readonly ClassStats _classStats;

        public const int DefaultHealth = 100;
        public const int DefaultDamage = 10;
        public const int DefaultAvoid = 10;
        public const int DefaultCrit = 10;

        public ShipGameOutcome(ClassStats classStats)
        {
            _classStats = classStats;
        }

        public int GetHealth(Account userdata)
        {

        }

        public int GetHealth(Account userdata, GameEnemy enemydata)
        {

        }

        public int GetDamage(Account userdata)
        {

        }

        public int GetDamage(Account userdata, GameEnemy enemydata)
        {

        }
    }
}
