using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Humanizer;
using Qmmands;

namespace Hanekawa.Bot.Modules.Info
{
    [Name("Info")]
    [Description("Commands for delivering information about the bot")]
    public class Info : HanekawaModule
    {
        [Name("About")]
        [Command("about", "info", "bot", "botinfo")]
        [Description("General information about the bot and links")]
        [RequireBotGuildPermissions(Permission.EmbedLinks)]
        [Cooldown(1, 5, CooldownMeasure.Seconds, HanaCooldown.Whatever)]
        [RequiredChannel]
        public async Task AboutAsync()
        {
            // var appData = await Context.Bot.GetCurrentApplicationAsync();

            var embed = new LocalEmbedBuilder
            {
                Author = new LocalEmbedAuthorBuilder { Name = Context.Bot.CurrentUser.Name, IconUrl = Context.Bot.CurrentUser.GetAvatarUrl() },
                Fields =
                {
                    new LocalEmbedFieldBuilder { Name = "Up Time", Value = (DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize(3), IsInline = true },
                    new LocalEmbedFieldBuilder { Name = "Support", Value = "[Invite Link](https://discord.gg/gGu5TT6)", IsInline = true },
                    new LocalEmbedFieldBuilder { Name = "Bot Invite", Value = "[Invite Link](https://discordapp.com/api/oauth2/authorize?client_id=431610594290827267&scope=bot&permissions=8)", IsInline = true }
                }
            }.Create("Hanekawa is a bot focused on rewarding user activity while giving server owners and moderators the tools they need for moderation, short and long-term.", Context.Colour.Get(Context.Guild.Id.RawValue));
            await Context.ReplyAsync(embed);
        }

        [Name("Up Time")]
        [Command("uptime")]
        [Description("Show bots up time")]
        [RequireBotGuildPermissions(Permission.EmbedLinks)]
        [Cooldown(1, 5, CooldownMeasure.Seconds, HanaCooldown.Whatever)]
        [RequiredChannel]
        public async Task UptimeAsync() 
            => await Context.ReplyAsync($"Current up time: {(DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize()}");
    }
}