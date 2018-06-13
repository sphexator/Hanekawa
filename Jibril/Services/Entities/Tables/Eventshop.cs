using System;
using System.Collections.Generic;

namespace Jibril
{
    public partial class Eventshop
    {
        public int Id { get; set; }
        public string Item { get; set; }
        public int? Price { get; set; }
        public int? Stock { get; set; }
    }
}
