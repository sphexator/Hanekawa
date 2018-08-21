using Newtonsoft.Json;

namespace Hanekawa.Anilist.Internal.Queries
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