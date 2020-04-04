using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity;
using Hanekawa.Bot.Services.ImageGen;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Qmmands;
using Quartz.Util;

namespace Hanekawa.Bot.Modules.Settings
{
    [Name("Welcome")]
    [RequireBotGuildPermissions(Permission.EmbedLinks, Permission.AttachFiles)]
    public class Welcome : HanekawaModule
    {
        private readonly ImageGenerator _image;
        public Welcome(ImageGenerator image) => _image = image;

        [Name("Banner Add")]
        [Command("wa", "welcadd")]
        [Description("Adds a welcome banner to the bot")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task AddWelcomeBannerAsync(string url)
        {
            if (!url.IsPictureUrl())
            {
                await Context.ReplyAsync("Please use direct image urls when adding pictures!\n" +
                                         "Example: <https://hanekawa.moe/hanekawa/0003.jpg>", Color.Red);
                return;
            }

            var example = await _image.WelcomeBuilder(Context.Member, url);
            example.Position = 0;
            var msg = await Context.Channel.SendMessageAsync(new LocalAttachment(example, "WelcomeExample.png"),
                "Do you want to add this banner? (y/N)");
            var response = await Context.Bot.GetInteractivity().WaitForMessageAsync(x =>
                x.Message.Author == Context.Member && x.Message.Guild == Context.Guild && x.Message.Channel == Context.Channel, TimeSpan.FromMinutes(2));
            if (response == null || response.Message.Content.ToLower() != "y")
            {
                await msg.DeleteAsync();
                await Context.ReplyAndDeleteAsync(null, false,
                    new LocalEmbedBuilder().Create("Aborting...", Color.Red),TimeSpan.FromSeconds(20));
                return;
            }

            using (var db = new DbService())
            {
                var data = new WelcomeBanner
                {
                    GuildId = Context.Guild.Id,
                    UploadTimeOffset = new DateTimeOffset(DateTime.UtcNow),
                    Uploader = Context.User.Id,
                    Url = url
                };
                await db.WelcomeBanners.AddAsync(data);
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Added banner to the collection!", Color.Green);
            }
        }

        [Name("Banner Remove")]
        [Command("wr", "welcremove")]
        [Description("Removes a welcome banner by given ID")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task RemoveWelcomeBannerAsync(int id)
        {
            using (var db = new DbService())
            {
                var banner =
                    await db.WelcomeBanners.FirstOrDefaultAsync(x => x.Id == id && x.GuildId == Context.Guild.Id);
                if (banner == null)
                {
                    await Context.ReplyAsync("Couldn\'t remove a banner with that ID.", Color.Red);
                    return;
                }

                db.WelcomeBanners.Remove(banner);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Removed {banner.Url} with ID {banner.Id} from the bot",
                    Color.Green);
            }
        }

        [Name("Banner List")]
        [Command("wl", "welclist")]
        [Description("Shows a paginated message of all saved banners")]
        [RequireMemberGuildPermissions(Permission.ManageMessages)]
        public async Task WelcomeBannerListAsync()
        {
            using (var db = new DbService())
            {
                var list = await db.WelcomeBanners.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                if (list.Count == 0)
                {
                    await Context.ReplyAsync("No banners added, using default one if enabled", Color.Red);
                    return;
                }

                var pages = new List<string>();
                for (var i = 0; i < list.Count; i++)
                {
                    var index = list[i];
                    var strBuilder = new StringBuilder();
                    strBuilder.AppendLine($"ID: {index.Id}");
                    strBuilder.AppendLine($"URL: {index.Url}");
                    strBuilder.AppendLine(
                        $"Uploader: {Context.Guild.GetMember(index.Uploader).Mention ?? $"User left server ({index.Uploader})"}");
                    strBuilder.AppendLine($"Added: {index.UploadTimeOffset.DateTime}");
                    pages.Add(strBuilder.ToString());
                }

                await Context.PaginatedReply(pages, Context.Guild, $"Welcome banners for {Context.Guild.Name}");
            }
        }

        [Name("Message")]
        [Command("wm", "welcmsg")]
        [Description("Sets welcome message")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task WelcomeMessageAsync([Remainder] string message = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateWelcomeConfigAsync(Context.Guild);
                cfg.Message = message.IsNullOrWhiteSpace() ? null : message;
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Updated welcome message!", Color.Green);
            }
        }

        [Name("Channel")]
        [Command("wc", "welchannel")]
        [Description("Sets welcome channel, leave empty to disable")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task WelcomeChannelAsync(CachedTextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateWelcomeConfigAsync(Context.Guild);
                if (channel == null && cfg.Channel.HasValue)
                {
                    cfg.Channel = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Disabled welcome messages!", Color.Green);
                }
                else if (channel == null)
                {
                    channel = Context.Channel;
                    cfg.Channel = channel.Id;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"Set welcome channel to {channel.Mention}", Color.Green);
                }
                else
                {
                    cfg.Channel = channel.Id;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"Enabled or changed welcome channel to {channel.Mention}", Color.Green);
                }
            }
        }

        [Name("Auto Delete")]
        [Command("wad", "welcautodel")]
        [Description("A timeout for when welcome messages are automatically deleted. Leave empty to disable")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task WelcomeTimeout(TimeSpan? timeout = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateWelcomeConfigAsync(Context.Guild);
                if (!cfg.TimeToDelete.HasValue && timeout == null) return;
                if (timeout == null)
                {
                    cfg.TimeToDelete = null;
                    await Context.ReplyAsync("Disabled auto-deletion of welcome messages!", Color.Green);
                }
                else
                {
                    cfg.TimeToDelete = timeout.Value;
                    await Context.ReplyAsync("Enabled auto-deletion of welcome messages!\n" +
                                             $"I will now delete the message after {timeout.Value.Humanize(2)}!",
                        Color.Green);
                }

                await db.SaveChangesAsync();
            }
        }

        [Name("Template")]
        [Command("wt", "welctemplate")]
        [Description("Posts the welcome template to create welcome banners from. PSD and regular png file.")]
        [RequireMemberGuildPermissions(Permission.ManageMessages)]
        public async Task WelcomeTemplate()
        {
            var embed = new LocalEmbedBuilder()
                .Create(
                    "The PSD file contains everything that's needed to get started creating your own banners.\n" +
                    "Below you see a preview of how the template looks like in plain PNG format, which you can use in case you're unable to open PSD files.\n" +
                    "The dimension or resolution for a banner is 600px wide and 78px height (600x78)", Context.Colour.Get(Context.Guild.Id))
                .WithTitle("Welcome template")
                .WithImageUrl("https://i.imgur.com/rk5BBmf.png");
            await Context.Channel.SendMessageAsync(new LocalAttachment("Data/Welcome/WelcomeTemplate.psd", "WelcomeTemplate.psd"), null, false, embed.Build());
        }

        [Name("Banner Toggle")]
        [Command("wbt", "welcbantog")]
        [Description("Toggles whether welcome banners should be posted or just message")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task Welcomebanner()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateWelcomeConfigAsync(Context.Guild);
                if (cfg.Banner)
                {
                    cfg.Banner = false;
                    await Context.ReplyAsync("Disabled welcome banners!", Color.Green);
                }
                else
                {
                    cfg.Banner = true;
                    await Context.ReplyAsync("Enabled welcome banners!", Color.Green);
                }

                await db.SaveChangesAsync();
            }
        }

        [Name("Ignore New Account")]
        [Command("wia")]
        [Description("Sets if welcomes should ignore new accounts by a defined time. Disabled by default")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task WelcomeIgnoreUsers(DateTimeOffset? time = null)
        {
            using var db = new DbService();
            var cfg = await db.GetOrCreateWelcomeConfigAsync(Context.Guild);
            if (time == null)
            {
                cfg.IgnoreNew = null;
                await Context.ReplyAsync("No longer ignoring new accounts on welcome", Color.Green);
            }
            else
            {
                cfg.IgnoreNew = time.Value;
                await Context.ReplyAsync($"Now ignoring accounts that's younger than {time.Value.Humanize()}",
                    Color.Green);
            }

            await db.SaveChangesAsync();
        }
    }
}