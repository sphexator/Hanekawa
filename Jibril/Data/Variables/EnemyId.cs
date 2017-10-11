using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Data.Variables
{
    public class EnemyId
    {
        public int Id { get; set; }
        public string EnemyName { get; set; }
        public string ImagePath { get; set; }
        public int Health { get; set; }
        public int Damage { get; set; }
        public string EnemyClass { get; set; }
        public int ExpGain { get; set; }
        public int CurrenyGain { get; set; }
    }
}
