﻿using System;

namespace Hanekawa.AnimeSimulCast.Entity
{
    public class AnimeData
    {
        public string Title { get; set; }
        public string Episode { get; set; }
        public string Season { get; set; }
        public string Url { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}
