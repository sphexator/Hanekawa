using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Modules.Administration.List
{
    public class WarnList
    {
        public string Staff_id { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
    }

    public class WarnAmount
    {
        public int Warnings { get; set; }
        public int Total_warnings { get; set; }
    }
}
