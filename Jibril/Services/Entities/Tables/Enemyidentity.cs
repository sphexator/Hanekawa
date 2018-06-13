using System;
using System.Collections.Generic;

namespace Jibril
{
    public partial class Enemyidentity
    {
        public int Id { get; set; }
        public string EnemyName { get; set; }
        public string ImageName { get; set; }
        public int? Health { get; set; }
        public int? Damage { get; set; }
        public string EnemyClass { get; set; }
        public int? ExpGain { get; set; }
        public int? CurrencyGain { get; set; }
    }
}
