using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.ImageGen;
using Hanekawa.Core.Interactive;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Qmmands;
using Quartz.Util;

namespace Hanekawa.Bot.Modules.Settings
{
    [Name("Welcome")]
    [RequireBotPermission(GuildPermission.EmbedLinks, GuildPermission.AttachFiles)]
    public class Welcome : InteractiveBase
    {
        private readonly ImageGenerator _image;
        public Welcome(ImageGenerator image) => _image = image;

        [Name("Welcome Banner")]
        [Command("welcome add", "welc add")]
        [Description("Adds a welcome banner to the bot")]
        [Remarks("welcome add imgur.com")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task AddWelcomeBannerAsync(string url)
        {
            if (!url.IsPictureUrl())
            {
                await Context.ReplyAsync("Please use direct image urls when adding pictures!\n" +
                                         "Example: <https://hanekawa.moe/hanekawa/0003.jpg>", Color.Red.RawValue);
                return;
            }

            var example = await _image.WelcomeBuilder(Context.User, url);
            example.Position = 0;
            var msg = await Context.Channel.SendFileAsync(example, "WelcomeExample.png",
                "Do you want to add this banner? (y/N)");
            var response = await NextMessageAsync(true, true, TimeSpan.FromMinutes(2));
            if (response == null || response.Content.ToLower() != "n")
            {
                await msg.TryDeleteMessageAsync();
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().CreateDefault("Aborting...", Color.Red.RawValue).Build(),
                    TimeSpan.FromSeconds(20));
                return;
            }
            using (var db = new DbService())
            {
                var data = new WelcomeBanner
                {
                    GuildId = Context.Guild.Id,
                    UploadTimeOffset = new DateTimeOffset(DateTime.UtcNow),
                    Uploader = Context.User.Id,
                    Url = url,
                };
                await db.WelcomeBanners.AddAsync(data);
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Added banner to the collection!", Color.Green.RawValue);
            }
        }

        [Name("Welcome Banner Remove")]
        [Command("welcome remove", "welc remove")]
        [Description("Removes a welcome banner by given ID")]
        [Remarks("welcome remove 123")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemoveWelcomeBannerAsync(int id)
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

        [Name("Welcome Message")]
        [Command("welcome message", "welc msg")]
        [Description("Sets welcome message")]
        [Remarks("welcome message Welcome %USER% to %GUILD% !")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task WelcomeMessageAsync([Remainder] string message)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateWelcomeConfigAsync(Context.Guild);
                cfg.Message = message.IsNullOrWhiteSpace() ? null : message;
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Updated welcome message!", Color.Green.RawValue);
            }
        }

        [Name("Welcome Channel")]
        [Command("welcome channel", "welc channel")]
        [Description("Sets welcome channel, leave empty to disable")]
        [Remarks("welcome channel #general")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task WelcomeChannelAsync(SocketTextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateWelcomeConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.Channel = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Disabled welcome messages!", Context.Guild.Id);
                }
                else
                {
                    cfg.Channel = channel.Id;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"Enabled or changed welcome messages to {channel.Mention}",
                        Context.Guild.Id);
                }
            }
        }

        [Name("Welcome Auto Delete")]
        [Command("welcome autodelete", "welc autodel")]
        [Description("A timeout for when welcome messages are automatically deleted. Leave empty to disable")]
        [Remarks("welcome autodelete 5m")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task WelcomeTimeout(TimeSpan? timeout = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateWelcomeConfigAsync(Context.Guild);
                if (!cfg.TimeToDelete.HasValue && timeout == null) return;
                if (timeout == null)
                {
                    cfg.TimeToDelete = null;
                    await Context.ReplyAsync("Disabled auto-deletion of welcome messages!", Color.Green.RawValue);
                }
                else
                {
                    cfg.TimeToDelete = timeout.Value;
                    await Context.ReplyAsync("Enabled auto-deletion of welcome messages!\n" +
                                             $"I will now delete the message after {timeout.Value.Humanize()}!",
                        Color.Green.RawValue);
                }

                await db.SaveChangesAsync();
            }
        }

        [Name("Welcome Template")]
        [Command("welcome template", "welc template")]
        [Description("Posts the welcome template to create welcome banners from. PSD and regular png file.")]
        [Remarks("welcome template")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task WelcomeTemplate()
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

        [Name("Welcome banner toggle")]
        [Command("welcome banner", "welc banner")]
        [Description("Toggles whether welcome banners should be posted or just message")]
        [Remarks("welcome banner")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task Welcomebanner()
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

        [Name("Welcome Banner List")]
        [Command("welcome list", "welc list")]
        [Description("Shows a paginated message of all saved banners")]
        [Remarks("welcome list")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task WelcomeBannerListAsync()
        {
            using (var db = new DbService())
            {
                var list = await db.WelcomeBanners.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                if (list.Count == 0)
                {
                    await Context.ReplyAsync("No banners added, using default one if enabled", Color.Red.RawValue);
                    return;
                }

                var pages = new List<string>();
                for (var i = 0; i < list.Count; i++)
                {
                    var index = list[i];
                    pages.Add($"ID: {index.Id}\n" +
                              $"URL: {index.Url}\n" +
                              $"Uploader: {Context.Guild.GetUser(index.Uploader).Mention ?? $"User left server ({index.Uploader})"}\n" +
                              $"Added: {index.UploadTimeOffset.DateTime}\n" +
                              "\n");
                }

                await PagedReplyAsync(pages.PaginateBuilder(Context.Guild, $"Welcome banners for {Context.Guild.Name}",
                    null, 5, db));
            }
        }
    }
}
