﻿using System.Collections.Generic;

namespace Hanekawa.Models
{
    public class Module
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Command> Commands { get; set; }
    }
}