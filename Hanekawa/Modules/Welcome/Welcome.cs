using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Config;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Services.Welcome;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Quartz.Util;

namespace Hanekawa.Modules.Welcome
{
    [Name("welcome")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public class Welcome : InteractiveBase
    {
        private readonly ImageGenerator _banner;
        private readonly WelcomeMessage _message;

        public Welcome(ImageGenerator banner, WelcomeMessage message)
        {
            _banner = banner;
            _message = message;
        }

        [Name("Welcome add")]
        [Command("welcome add", RunMode = RunMode.Async)]
        [Alias("welc add")]
        [Summary("Adds a banner to the bot")]
        [Remarks("h.welc add imgur.com")]
        public async Task AddWelcomeBanner(string url)
        {
            if (!url.IsPictureUrl())
            {
                await Context.ReplyAsync("Please use direct image urls when adding pictures!\n" +
                                         "Example: <https://hanekawa.moe/hanekawa/0003.jpg>", Color.Red.RawValue);
                return;
            }

            var stream = await _banner.TestBanner(Context.User as IGuildUser, url);
            stream.Seek(0, SeekOrigin.Begin);
            var msg = await Context.Channel.SendFileAsync(stream, "Welcome.png",
                "Do you want to add this banner? (Y/N");
            var response = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));
            if (response == null) return;
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

        [Name("Welcome remove")]
        [Command("welcome remove", RunMode = RunMode.Async)]
        [Alias("welc remove")]
        [Summary("Removes a banner from the bot")]
        [Remarks("h.welc remove 5")]
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

        [Name("Welcome list")]
        [Command("welcome list", RunMode = RunMode.Async)]
        [Alias("welc list")]
        [Summary("Lists all banners for this guild")]
        [Remarks("h.welc list")]
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
                    pages.Add($"ID: {x.Id}\n" +
                              $"URL: {x.Url}\n" +
                              $"Uploader: {Context.Guild.GetUser(x.Uploader).Mention ?? $"User left server ({x.Uploader})"}\n" +
                              $"Added: {x.UploadTimeOffset.DateTime}\n" +
                              "\n");

                await PagedReplyAsync(pages.PaginateBuilder(Context.Guild.Id, Context.Guild,
                    $"Welcome banners for {Context.Guild.Name}"));
            }
        }

        [Name("Welcome test")]
        [Command("welcome test", RunMode = RunMode.Async)]
        [Alias("welc test")]
        [Summary("Tests a banner from a url to see how it looks")]
        [Remarks("h.welc test")]
        public async Task TestWelcomeBanner(string url)
        {
            if (!url.IsPictureUrl())
            {
                await Context.ReplyAsync("Please use direct image urls when adding pictures!\n" +
                                         "Example: <https://hanekawa.moe/hanekawa/0003.jpg>", Color.Red.RawValue);
                return;
            }

            var stream = await _banner.TestBanner(Context.User as IGuildUser, url);
            stream.Seek(0, SeekOrigin.Begin);
            await Context.Channel.SendFileAsync(stream, "welcome.png");
        }

        [Name("Welcome template")]
        [Command("welcome template", RunMode = RunMode.Async)]
        [Alias("welc template")]
        [Summary("Sends banner template")]
        [Remarks("h.welc template")]
        public async Task TemplateWelcomeBanner()
        {
            var embed = new EmbedBuilder()
                .CreateDefault(
                    "The PSD file contains everything that's needed to get started creating your own banners.\n" +
                    "Below you see a preview of how the template looks like in plain PNG format, which you can use in case you're unable to open PSD files.\n" +
                    "The dimension or resolution for a banner is 600px wide and 78px height (600x78)", Context.Guild.Id)
                .WithTitle("Welcome template")
                .WithImageUrl("https://i.imgur.com/rk5BBmf.png");
            await Context.Channel.SendFileAsync("Data/Welcome/WelcomeTemplate.psd", null, false, embed.Build());
        }

        [Name("Welcome message")]
        [Command("welcome message", RunMode = RunMode.Async)]
        [Alias("welc msg")]
        [Summary("Sets welcome message")]
        [Remarks("h.welc msg Welcome %user% to %guild%")]
        public async Task SetWelcomeMessage([Remainder] string message)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateWelcomeConfigAsync(Context.Guild);
                cfg.Message = message.IsNullOrWhiteSpace() ? null : message;
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Updated welcome message!\n\n" +
                                         $"{_message.Message(message, Context.User, Context.Guild)}",
                    Color.Green.RawValue);
            }
        }

        [Name("Welcome auto delete")]
        [Command("welcome autodelete", RunMode = RunMode.Async)]
        [Alias("welc autodel")]
        [Summary("Sets when a welcome message should delete on its own")]
        [Remarks("h.autodel 5m")]
        public async Task SetAutoDeleteTimer(TimeSpan? timer = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateWelcomeConfigAsync(Context.Guild);
                if (!cfg.TimeToDelete.HasValue && timer == null) return;
                if (timer == null)
                {
                    cfg.TimeToDelete = null;
                    await Context.ReplyAsync("Disabled auto-deletion of welcome messages!", Color.Green.RawValue);
                }
                else
                {
                    cfg.TimeToDelete = timer.Value;
                    await Context.ReplyAsync("Enabled auto-deletion of welcome messages!\n" +
                                             $"I will now delete the message after {timer.Value.Humanize()}!",
                        Color.Green.RawValue);
                }

                await db.SaveChangesAsync();
            }
        }

        [Name("Welcome banner")]
        [Command("welcome banner", RunMode = RunMode.Async)]
        [Alias("welc banner")]
        [Summary("Toggles welcome banner")]
        [Remarks("h.welc banner")]
        public async Task ToggleBannerWelcomeBanner()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateWelcomeConfigAsync(Context.Guild);
                if (cfg.Banner)
                {
                    cfg.Banner = false;
                    await Context.ReplyAsync("Disabled welcome banners!", Color.Green.RawValue);
                }
                else
                {
                    cfg.Banner = true;
                    await Context.ReplyAsync("Enabled welcome banners!", Color.Green.RawValue);
                }

                await db.SaveChangesAsync();
            }
        }

        [Name("Welcome channel")]
        [Command("welcome channel", RunMode = RunMode.Async)]
        [Alias("welc channel")]
        [Summary("Enables or disables welcome messages in a channel")]
        [Remarks("h.welc channel #general")]
        public async Task ToggleWelcome(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateWelcomeConfigAsync(Context.Guild);
                if (cfg.Channel.HasValue && channel == null)
                {
                    cfg.Channel = null;
                    await Context.ReplyAsync("disabled welcome notifications!", Color.Green.RawValue);
                }
                else if (cfg.Channel.HasValue && channel != null)
                {
                    cfg.Channel = channel.Id;
                    await Context.ReplyAsync($"Enabled welcome notifications in {channel.Mention}!",
                        Color.Green.RawValue);
                }
                else if (!cfg.Channel.HasValue && channel == null)
                {
                    if (Context.Channel is ITextChannel textChannel) channel = textChannel;
                    else return;
                    cfg.Channel = channel.Id;
                    await Context.ReplyAsync($"Enabled welcome notifications in {channel.Mention}!",
                        Color.Green.RawValue);
                }
                else if (!cfg.Channel.HasValue)
                {
                    cfg.Channel = channel.Id;
                    await Context.ReplyAsync($"Enabled welcome notifications in {channel.Mention}!",
                        Color.Green.RawValue);
                }
                else
                {
                    cfg.Channel = null;
                    await Context.ReplyAsync("disabled welcome notifications!", Color.Green.RawValue);
                }

                await db.SaveChangesAsync();
            }
        }
    }
}