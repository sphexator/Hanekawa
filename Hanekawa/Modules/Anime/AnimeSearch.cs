using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Miki.Anilist;

namespace Hanekawa.Modules.Anime
{
    public class AnimeSearch : InteractiveBase
    {
        [Command("anime search")]
        [RequireOwner]
        public async Task AniSearchAsync([Remainder] string anime)
        {
            var client = new AnilistClient();
            var animeResult = await client.GetMediaAsync(anime, MediaFormat.TV);
            await ReplyAsync(animeResult.EnglishTitle);
        }
    }
}