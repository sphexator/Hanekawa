using System;
using System.Collections.Generic;

namespace Jibril
{
    public partial class Modlog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Msgid { get; set; }
        public string Responduser { get; set; }
        public string Response { get; set; }
        public string Date { get; set; }
    }
}
