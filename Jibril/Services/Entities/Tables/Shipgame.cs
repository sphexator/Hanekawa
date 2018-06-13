using System;
using System.Collections.Generic;

namespace Jibril
{
    public partial class Shipgame
    {
        public string UserId { get; set; }
        public int? Health { get; set; }
        public int? Damagetaken { get; set; }
        public int? Combatstatus { get; set; }
        public int? Enemyid { get; set; }
        public int? EnemyDamageTaken { get; set; }
        public int? Enemyhealth { get; set; }
        public int? KillAmount { get; set; }
    }
}
