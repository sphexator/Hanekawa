using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Hanekawa.Addons.Anilist.Objects
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MediaFormat
    {
        TV,
        OVA,
        ONA,
        MANGA,
        SPECIAL,
        TV_SHORT,
        ONE_SHOT,
        MUSIC,
        MOVIE,
        NOVEL
    }
}