﻿using Newtonsoft.Json;

namespace Hanekawa.Anilist.Internal
{
    internal class AnilistName
    {
        [JsonProperty("first")] internal string First;

        [JsonProperty("last")] internal string Last;

        [JsonProperty("native")] internal string Native;
    }
}