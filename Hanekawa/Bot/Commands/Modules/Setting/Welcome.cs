using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Rest;
using Hanekawa.Bot.Commands.Preconditions;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Bot.Service.ImageGeneration;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Entities.Color;
using Hanekawa.Extensions;
using Hanekawa.Utility;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using Quartz.Util;

namespace Hanekawa.Bot.Commands.Modules.Setting
{
    [Name("Welcome")]
    [Description("Configure the welcome service")]
    [Group("Welcome", "Welc")]
    public class Welcome : HanekawaCommandModule
    {
        [Name("Banner Add")]
        [Command("add")]
        [Description("Adds a welcome banner to the bot")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task<DiscordCommandResult> AddWelcomeBannerAsync(string url, int aviSize = 60, int aviX = 10,
            int aviY = 10, int textSize = 33, int textX = 245, int textY = 40)
        {
            if (!url.IsPictureUrl())
                return Reply("Please use direct image urls when adding pictures!\n" +
                             "Example: <https://hanekawa.moe/images/67531066_p2.png>",
                    HanaBaseColor.Bad()); // TODO: Use own images

            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var guildCfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
            await Context.Channel.TriggerTypingAsync();
            var (stream, _) = await Context.Services.GetRequiredService<ImageGenerationService>()
                .WelcomeAsync(Context.Author, url, aviSize, aviX, aviY, textSize, textX, textY,
                    guildCfg.Premium >= DateTimeOffset.UtcNow);
            stream.Position = 0;
            var fileName = "welcomeExample.png";
            if (url.EndsWith(".gif")) fileName = "welcomeExample.gif";
            var msg = await Reply(new LocalMessage
            {
                Content = "Do you want to add this banner? (y/N)\n" +
                          "You can adjust placement of avatar and text by adjust these values in the command (this is the full command with default values)\n" +
                          $"**Example:** wa {url.Truncate(5)} {aviSize} {aviX} {aviY} {textSize} {textX} {textY}\n" +
                          $"**Explained:** wa {url.Truncate(5)} PfpSize = {aviSize} PfpX = {aviX} PfpY = {aviY} TextSize = {textSize} TextX = {textX} TextY = {textY}",
                Attachments = {new LocalAttachment(stream, fileName)}
            });
            var response = await Context.Bot.GetInteractivity().WaitForMessageAsync(Context.ChannelId, x =>
                x.Message.Author == Context.Author, TimeSpan.FromMinutes(2));
            if (response == null || response.Message.Content.ToLower() != "y")
            {
                await msg.DeleteAsync();
                await ReplyAndDeleteAsync(
                    new LocalMessage().Create("Aborting...", HanaBaseColor.Bad()), TimeSpan.FromSeconds(20));
                return null;
            }

            await db.WelcomeBanners.AddAsync(new WelcomeBanner
            {
                GuildId = Context.Guild.Id,
                UploadTimeOffset = new DateTimeOffset(DateTime.UtcNow),
                Uploader = Context.Author.Id,
                Url = url,
                IsNsfw = false
            });
            await db.SaveChangesAsync();
            return Reply("Added banner to the collection!", Color.Green);
        }

        [Name("Banner Remove")]
        [Command("remove")]
        [Description("Removes a welcome banner by given ID")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task<DiscordCommandResult> RemoveWelcomeBannerAsync(int id)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var banner =
                await db.WelcomeBanners.FirstOrDefaultAsync(x => x.Id == id && x.GuildId == Context.Guild.Id);
            if (banner == null) return Reply("Couldn\'t remove a banner with that ID.", HanaBaseColor.Bad());

            db.WelcomeBanners.Remove(banner);
            await db.SaveChangesAsync();
            return Reply($"Removed {banner.Url} with ID {banner.Id} from the bot", HanaBaseColor.Ok());
        }

        [Name("Banner List")]
        [Command("list")]
        [Description("Shows a paginated message of all saved banners")]
        [RequireAuthorGuildPermissions(Permission.ManageMessages)]
        public async Task<DiscordCommandResult> WelcomeBannerListAsync()
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var list = await db.WelcomeBanners.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
            if (list.Count == 0)
                return Reply("No banners added, using default one if enabled",
                    Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId));

            var pages = new List<string>();
            foreach (var index in list)
            {
                var str = new StringBuilder();
                str.AppendLine($"ID: {index.Id}");
                str.AppendLine($"URL: {index.Url}");
                str.AppendLine(
                    $"Uploader: {(await Context.Guild.GetOrFetchMemberAsync(index.Uploader)).Mention ?? $"User left server ({index.Uploader})"}");
                str.AppendLine($"Added: {index.UploadTimeOffset.DateTime}");
                pages.Add(str.ToString());
            }

            return Pages(pages.Pagination(
                Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId),
                Context.Guild.GetIconUrl(), $"Welcome banners for {Context.Guild.Name}"));
        }

        [Name("Message")]
        [Command("msg")]
        [Description("Sets welcome message")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task<DiscordCommandResult> WelcomeMessageAsync([Remainder] string message = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateWelcomeConfigAsync(Context.Guild);
            cfg.Message = message.IsNullOrWhiteSpace() ? null : message;
            await db.SaveChangesAsync();
            return Reply("Updated welcome message!\n" +
                         $"Example: {MessageUtil.FormatMessage(message, Context.Author, Context.Guild)}",
                HanaBaseColor.Ok());
        }

        [Name("Channel")]
        [Command("channel")]
        [Description("Sets welcome channel, leave empty to disable")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task<DiscordCommandResult> WelcomeChannelAsync(ITextChannel channel = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateWelcomeConfigAsync(Context.Guild);
            switch (channel)
            {
                case null when cfg.Channel.HasValue:
                    cfg.Channel = null;
                    await db.SaveChangesAsync();
                    return Reply("Disabled welcome messages!", Color.Green);
                case null:
                {
                    channel = Context.Channel;
                    if (channel == null) return null;
                    cfg.Channel = channel.Id;
                    await db.SaveChangesAsync();
                    return Reply($"Set welcome channel to {channel.Mention}", Color.Green);
                }
                default:
                    cfg.Channel = channel.Id;
                    await db.SaveChangesAsync();
                    return Reply($"Enabled or changed welcome channel to {channel.Mention}", Color.Green);
            }
        }

        [Name("Auto Delete")]
        [Command("autodelete", "autodel")]
        [Description("A timeout for when welcome messages are automatically deleted. Leave empty to disable")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task<DiscordCommandResult> WelcomeTimeout(TimeSpan? timeout = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateWelcomeConfigAsync(Context.Guild);
            if (!cfg.TimeToDelete.HasValue && timeout == null) return null;
            if (timeout == null)
            {
                cfg.TimeToDelete = null;
                await db.SaveChangesAsync();
                return Reply("Disabled auto-deletion of welcome messages!", HanaBaseColor.Ok());
            }

            cfg.TimeToDelete = timeout.Value;
            await db.SaveChangesAsync();
            return Reply("Enabled auto-deletion of welcome messages!\n" +
                         $"I will now delete the message after {timeout.Value.Humanize(2)}!", HanaBaseColor.Ok());
        }

        [Name("Template")]
        [Command("template")]
        [Description("Posts the welcome template to create welcome banners from. PSD and regular png file.")]
        [RequireAuthorGuildPermissions(Permission.ManageMessages)]
        public DiscordResponseCommandResult WelcomeTemplate()
        {
            return Reply(new LocalMessage
            {
                Embeds = new[]
                {
                    new LocalEmbed
                    {
                        Color = Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId),
                        Description =
                            "The PSD file contains everything that's needed to get started creating your own banners.\n" +
                            "Below you see a preview of how the template looks like in plain PNG format, which you can use in case you're unable to open PSD files.\n" +
                            "The dimension or resolution for a banner is 600px wide and 78px height (600x78)",
                        Title = "Welcome template",
                        ImageUrl = "https://i.imgur.com/rk5BBmf.png"
                    }
                },
                Attachments =
                {
                    new LocalAttachment(new FileStream("Data/Welcome/WelcomeTemplate.psd", FileMode.Open),
                        "WelcomeTemplate.psd")
                }
            });
        }

        [Name("Banner Toggle")]
        [Command("bannertoggle", "toggle")]
        [Description("Toggles whether welcome banners should be posted or just message")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task<DiscordCommandResult> Welcomebanner()
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateWelcomeConfigAsync(Context.Guild);
            if (cfg.Banner)
            {
                cfg.Banner = false;
                await db.SaveChangesAsync();
                return Reply("Disabled welcome banners!", HanaBaseColor.Ok());
            }

            cfg.Banner = true;
            await db.SaveChangesAsync();
            return Reply("Enabled welcome banners!", HanaBaseColor.Ok());
        }

        [Name("Ignore New Account")]
        [Command("ignore")]
        [Description("Sets if welcomes should ignore new accounts by a defined time. Disabled by default")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task<DiscordCommandResult> WelcomeIgnoreUsers(TimeSpan? time = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateWelcomeConfigAsync(Context.Guild);
            if (time == null)
            {
                cfg.IgnoreNew = null;
                await db.SaveChangesAsync();
                return Reply("No longer ignoring new accounts on welcome", HanaBaseColor.Ok());
            }

            cfg.IgnoreNew = time.Value;
            await db.SaveChangesAsync();
            return Reply($"Now ignoring accounts that's younger than {time.Value.Humanize()}",
                Color.Green);
        }

        [Name("Welcome Reward")]
        [Command("reward")]
        [Description("Rewards users for welcoming a new member")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        [RequirePremium]
        public async Task<DiscordCommandResult> WelcomeRewardAsync(int reward = 0)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateWelcomeConfigAsync(Context.Guild);
            if (reward == 0)
            {
                cfg.Reward = null;
                await db.SaveChangesAsync();
                return Reply("Disabled welcome rewards!", HanaBaseColor.Ok());
            }

            cfg.Reward = reward;
            await db.SaveChangesAsync();
            return Reply($"Enabled or set welcome rewards to {reward}!", HanaBaseColor.Ok());
        }
    }
}