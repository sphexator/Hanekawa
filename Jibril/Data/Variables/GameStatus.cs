using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Data.Variables
{
    public class GameStatus
    {
        public string UserID { get; set; }
        public int Health { get; set; }
        public int Damagetaken { get; set; }
        public int Combatstatus { get; set; }
        public int Enemyid { get; set; }
        public int EnemyDamageTaken { get; set; }
        public int Enemyhealth { get; set; }
        public int KillAmount { get; set; }
    }
}
