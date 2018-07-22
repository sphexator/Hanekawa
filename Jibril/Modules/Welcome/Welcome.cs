using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Extensions;
using Jibril.Services.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jibril.Modules.Welcome
{
    public class Welcome : InteractiveBase
    {
        //TODO: Fill all these methods
        [Group("welcome")]
        [Alias("welc")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public class WelcomeAdmin : InteractiveBase
        {
            [Command("add", RunMode = RunMode.Async)]
            [Summary("Adds a banner to the bot")]
            public async Task AddWelcomeBanner(string url)
            {

            }

            [Command("remove", RunMode = RunMode.Async)]
            [Summary("Removes a banner from the bot")]
            public async Task RemoveWelcomeBanner(int id)
            {
                using (var db = new DbService())
                {
                    var banner = await db.WelcomeBanners.FirstOrDefaultAsync(x => x.Id == id);
                    if (banner == null)
                    {
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply($"Couldn't remove a banner with that ID.", Color.Red.RawValue)
                                .Build());
                        return;
                    }

                    db.WelcomeBanners.Remove(banner);
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Removed {banner.Url} with ID {banner.Id} from the bot", Color.Green.RawValue)
                            .Build());
                }
            }

            [Command("list", RunMode = RunMode.Async)]
            [Summary("Lists all banners for this guild")]
            public async Task ListWelcomeBanner()
            {
                using (var db = new DbService())
                {
                    var list = db.WelcomeBanners.Where(x => x.GuildId == Context.Guild.Id);
                }
            }

            [Command("test", RunMode = RunMode.Async)]
            [Summary("Tests a banner from a url to see how it looks")]
            public async Task TestWelcomeBanner(string url)
            {

            }

            [Command("template", RunMode = RunMode.Async)]
            [Summary("Sends banner template")]
            public async Task TemplateWelcomeBanner()
            {

            }

            [Command("message", RunMode = RunMode.Async)]
            [Alias("msg")]
            [Summary("Sets welcome message")]
            public async Task SetWelcomeMessage([Remainder]string message)
            {

            }

            [Command("banner", RunMode = RunMode.Async)]
            [Summary("Toggles welcome banner")]
            public async Task ToggleBannerWelcomeBanner()
            {

            }

            [Command("toggle", RunMode = RunMode.Async)]
            [Summary("Toggles welcome messages")]
            public async Task ToggleWelcome()
            {

            }
        }
    }
}
