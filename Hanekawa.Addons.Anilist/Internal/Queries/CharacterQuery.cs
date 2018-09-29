using Newtonsoft.Json;

namespace Hanekawa.Addons.Anilist.Internal.Queries
{
    internal class CharacterQuery
    {
        [JsonProperty("Character")] internal AnilistCharacter Character;
    }

    internal class MediaQuery
    {
        [JsonProperty("Media")] internal AnilistMedia Media;
    }
}