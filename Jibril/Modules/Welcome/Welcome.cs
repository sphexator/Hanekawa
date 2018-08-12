using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Extensions;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Entities.Tables;
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
                await ReplyAsync(null, false, new EmbedBuilder().Reply("Please use direct image urls when adding pictures!\n" +
                                                                       "Example: <https://hanekawa.moe/hanekawa/0003.jpg>", Color.Red.RawValue).Build());
                return;
            }
            await _welcomeService.TestBanner(Context.Channel, Context.User as IGuildUser, url);
            await ReplyAsync($"Do you want to add this banner? (Y/N");
            var response = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));
            if (response.Content.ToLower() != "y")
            {
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Okay then :awaueyes:", Color.Red.RawValue).Build());
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
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Added banner to the collection!", Color.Green.RawValue).Build());
            }
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
                    new EmbedBuilder().Reply($"Removed {banner.Url} with ID {banner.Id} from the bot",
                            Color.Green.RawValue)
                        .Build());
            }
        }

        [Command("list", RunMode = RunMode.Async)]
        [Summary("Lists all banners for this guild")]
        public async Task ListWelcomeBanner()
        {
            using (var db = new DbService())
            {
                var list = await db.WelcomeBanners.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                var pages = new List<string>();
                for (var i = 0; i < list.Count;)
                {
                    string input = null;
                    for (var j = 0; j < 5; j++)
                    {
                        if (i >= list.Count) continue;
                        var entry = list[i];
                        input += $"ID: {entry.Id}\n" +
                                 $"URL: {entry.Url}\n" +
                                 $"Uploader: {Context.Guild.GetUser(entry.Uploader).Mention ?? $"User left server ({entry.Uploader})"}\n" +
                                 $"Added: {entry.UploadTimeOffset.DateTime}\n" +
                                 $"\n";
                        i++;
                    }

                    pages.Add(input);
                }

                var paginator = new PaginatedMessage
                {
                    Color = Color.DarkPurple,
                    Pages = pages,
                    Title = $"Welcome banners for {Context.Guild.Name}",
                    Options = new PaginatedAppearanceOptions
                    {
                        First = new Emoji("⏮"),
                        Back = new Emoji("◀"),
                        Next = new Emoji("▶"),
                        Last = new Emoji("⏭"),
                        Stop = null,
                        Jump = null,
                        Info = null
                    }
                };
                await PagedReplyAsync(paginator);
            }
        }

        [Command("test", RunMode = RunMode.Async)]
        [Summary("Tests a banner from a url to see how it looks")]
        public async Task TestWelcomeBanner(string url)
        {
            if (!url.IsPictureUrl())
            {
                await ReplyAsync(null, false, new EmbedBuilder().Reply("Please use direct image urls when adding pictures!\n" +
                                                                       "Example: <https://hanekawa.moe/hanekawa/0003.jpg>", Color.Red.RawValue).Build());
                return;
            }
            await _welcomeService.TestBanner(Context.Channel, Context.User as IGuildUser, url);
        }

        [Command("template", RunMode = RunMode.Async)]
        [Summary("Sends banner template")]
        public async Task TemplateWelcomeBanner()
        {
            await Context.Channel.SendFileAsync(@"Data\Welcome\WelcomeTemplate.psd", null, false,
                new EmbedBuilder().Reply("Welcome template.", Color.DarkPurple.RawValue).Build());
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
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply("Updated welcome message!", Color.Green.RawValue).Build());
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
                if (!(cfg.WelcomeDelete.HasValue) && timer == null) return;
                if (timer == null)
                {
                    cfg.WelcomeDelete = null;
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Disabled auto-deletion of welcome messages!", Color.Green.RawValue)
                            .Build());
                }
                else
                {
                    cfg.WelcomeDelete = timer.Value;
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Enabled auto-deletion of welcome messages!\n" +
                                                 $"I will now delete the message after {timer.Value.Humanize()}!", Color.Green.RawValue)
                            .Build());
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
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Disabled welcome banners!", Color.Green.RawValue).Build());
                }
                else
                {
                    cfg.WelcomeBanner = true;
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Enabled welcome banners!", Color.Green.RawValue).Build());
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
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("disabled welcome notifications!", Color.Green.RawValue).Build());
                }
                else if (cfg.WelcomeChannel.HasValue && channel != null)
                {
                    cfg.WelcomeChannel = channel.Id;
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Enabled welcome notifications in {channel.Mention}!",
                            Color.Green.RawValue).Build());
                }
                else if (!cfg.WelcomeChannel.HasValue && channel == null)
                {
                    if (Context.Channel is ITextChannel textChannel) channel = textChannel;
                    else return;
                    cfg.WelcomeChannel = channel.Id;
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Enabled welcome notifications in {channel.Mention}!",
                            Color.Green.RawValue).Build());
                }
                else if (!cfg.WelcomeChannel.HasValue)
                {
                    cfg.WelcomeChannel = channel.Id;
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Enabled welcome notifications in {channel.Mention}!",
                            Color.Green.RawValue).Build());
                }
                else
                {
                    cfg.WelcomeChannel = null;
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("disabled welcome notifications!", Color.Green.RawValue).Build());
                }

                await db.SaveChangesAsync();
            }
        }
    }
}