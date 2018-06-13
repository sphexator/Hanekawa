using System;
using System.Collections.Generic;

namespace Jibril
{
    public partial class Inventory
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int? RepairKit { get; set; }
        public int? DamageBoost { get; set; }
        public int? Shield { get; set; }
        public int? Customrole { get; set; }
        public int? Gift { get; set; }
    }
}
