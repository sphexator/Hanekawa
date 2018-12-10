using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Services.Welcome;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Quartz.Util;

namespace Hanekawa.Modules.Welcome
{
    [Group("welcome")]
    [Alias("welc")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class Welcome : InteractiveBase
    {
        private readonly WelcomeService _welcomeService;

        public Welcome(WelcomeService welcomeService)
        {
            _welcomeService = welcomeService;
        }

        [Command("add", RunMode = RunMode.Async)]
        [Summary("Adds a banner to the bot")]
        public async Task AddWelcomeBanner(string url)
        {
            if (!url.IsPictureUrl())
            {
                await Context.ReplyAsync("Please use direct image urls when adding pictures!\n" +
                                         "Example: <https://hanekawa.moe/hanekawa/0003.jpg>", Color.Red.RawValue);
                return;
            }

            await _welcomeService.TestBanner(Context.Channel, Context.User as IGuildUser, url);
            var msg = await ReplyAsync("Do you want to add this banner? (Y/N");
            var response = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));
            if (response.Content.ToLower() != "y")
            {
                await msg.DeleteAsync();
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().CreateDefault("Not adding.", Color.Red.RawValue).Build(),
                    TimeSpan.FromSeconds(15));
                return;
            }

            using (var db = new DbService())
            {
                var number = await db.WelcomeBanners.Where(x => x.GuildId == Context.Guild.Id).CountAsync();
                var data = new WelcomeBanner
                {
                    GuildId = Context.Guild.Id,
                    UploadTimeOffset = new DateTimeOffset(DateTime.UtcNow),
                    Uploader = Context.User.Id,
                    Url = url,
                    Id = number + 1
                };
                await db.WelcomeBanners.AddAsync(data);
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Added banner to the collection!", Color.Green.RawValue);
            }
        }

        [Command("remove", RunMode = RunMode.Async)]
        [Summary("Removes a banner from the bot")]
        public async Task RemoveWelcomeBanner(int id)
        {
            using (var db = new DbService())
            {
                var banner =
                    await db.WelcomeBanners.FirstOrDefaultAsync(x => x.Id == id && x.GuildId == Context.Guild.Id);
                if (banner == null)
                {
                    await Context.ReplyAsync("Couldn\'t remove a banner with that ID.", Color.Red.RawValue);
                    return;
                }

                db.WelcomeBanners.Remove(banner);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Removed {banner.Url} with ID {banner.Id} from the bot",
                    Color.Green.RawValue);
            }
        }

        [Command("list", RunMode = RunMode.Async)]
        [Summary("Lists all banners for this guild")]
        public async Task ListWelcomeBanner()
        {
            using (var db = new DbService())
            {
                var list = await db.WelcomeBanners.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                if (list.Count == 0)
                {
                    await Context.ReplyAsync("No banners added, using default one if enabled");
                    return;
                }

                var pages = new List<string>();
                foreach (var x in list)
                {
                    pages.Add($"ID: {x.Id}\n" +
                              $"URL: {x.Url}\n" +
                              $"Uploader: {Context.Guild.GetUser(x.Uploader).Mention ?? $"User left server ({x.Uploader})"}\n" +
                              $"Added: {x.UploadTimeOffset.DateTime}\n" +
                              "\n");
                }

                await PagedReplyAsync(pages.PaginateBuilder(Context.Guild,
                    $"Welcome banners for {Context.Guild.Name}"));
            }
        }

        [Command("test", RunMode = RunMode.Async)]
        [Summary("Tests a banner from a url to see how it looks")]
        public async Task TestWelcomeBanner(string url)
        {
            if (!url.IsPictureUrl())
            {
                await Context.ReplyAsync("Please use direct image urls when adding pictures!\n" +
                                         "Example: <https://hanekawa.moe/hanekawa/0003.jpg>", Color.Red.RawValue);
                return;
            }

            await _welcomeService.TestBanner(Context.Channel, Context.User as IGuildUser, url);
        }

        [Command("template", RunMode = RunMode.Async)]
        [Summary("Sends banner template")]
        public async Task TemplateWelcomeBanner()
        {
            var embed = new EmbedBuilder()
                .CreateDefault(
                    "The PSD file contains everything that's needed to get started creating your own banners.\n" +
                    "Below you see a preview of how the template looks like in plain PNG format, which you can use in case you're unable to open PSD files.\n" +
                    "The dimension or resolution for a banner is 600px wide and 78px height (600x78)")
                .WithTitle("Welcome template")
                .WithImageUrl("https://i.imgur.com/rk5BBmf.png");
            await Context.Channel.SendFileAsync(@"Data\Welcome\WelcomeTemplate.psd", null, false, embed.Build());
        }

        [Command("message", RunMode = RunMode.Async)]
        [Alias("msg")]
        [Summary("Sets welcome message")]
        public async Task SetWelcomeMessage([Remainder] string message)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                cfg.WelcomeMessage = message.IsNullOrWhiteSpace() ? null : message;
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Updated welcome message!", Color.Green.RawValue);
            }
        }

        [Command("autodelete", RunMode = RunMode.Async)]
        [Alias("autodel")]
        [Summary("Sets when a welcome message should delete on its own")]
        public async Task SetAutoDeleteTimer(TimeSpan? timer = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (!cfg.WelcomeDelete.HasValue && timer == null) return;
                if (timer == null)
                {
                    cfg.WelcomeDelete = null;
                    await Context.ReplyAsync("Disabled auto-deletion of welcome messages!", Color.Green.RawValue);
                }
                else
                {
                    cfg.WelcomeDelete = timer.Value;
                    await Context.ReplyAsync("Enabled auto-deletion of welcome messages!\n" +
                                             $"I will now delete the message after {timer.Value.Humanize()}!",
                        Color.Green.RawValue);
                }

                await db.SaveChangesAsync();
            }
        }

        [Command("banner", RunMode = RunMode.Async)]
        [Summary("Toggles welcome banner")]
        public async Task ToggleBannerWelcomeBanner()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (cfg.WelcomeBanner)
                {
                    cfg.WelcomeBanner = false;
                    await Context.ReplyAsync("Disabled welcome banners!", Color.Green.RawValue);
                }
                else
                {
                    cfg.WelcomeBanner = true;
                    await Context.ReplyAsync("Enabled welcome banners!", Color.Green.RawValue);
                }

                await db.SaveChangesAsync();
            }
        }

        [Command("toggle", RunMode = RunMode.Async)]
        [Summary("Toggles welcome messages")]
        public async Task ToggleWelcome(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (cfg.WelcomeChannel.HasValue && channel == null)
                {
                    cfg.WelcomeChannel = null;
                    await Context.ReplyAsync("disabled welcome notifications!", Color.Green.RawValue);
                }
                else if (cfg.WelcomeChannel.HasValue && channel != null)
                {
                    cfg.WelcomeChannel = channel.Id;
                    await Context.ReplyAsync($"Enabled welcome notifications in {channel.Mention}!",
                        Color.Green.RawValue);
                }
                else if (!cfg.WelcomeChannel.HasValue && channel == null)
                {
                    if (Context.Channel is ITextChannel textChannel) channel = textChannel;
                    else return;
                    cfg.WelcomeChannel = channel.Id;
                    await Context.ReplyAsync($"Enabled welcome notifications in {channel.Mention}!",
                        Color.Green.RawValue);
                }
                else if (!cfg.WelcomeChannel.HasValue)
                {
                    cfg.WelcomeChannel = channel.Id;
                    await Context.ReplyAsync($"Enabled welcome notifications in {channel.Mention}!",
                        Color.Green.RawValue);
                }
                else
                {
                    cfg.WelcomeChannel = null;
                    await Context.ReplyAsync("disabled welcome notifications!", Color.Green.RawValue);
                }

                await db.SaveChangesAsync();
            }
        }
    }
}