using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Humanizer;
using Qmmands;

namespace Hanekawa.Bot.Modules.Info
{
    [Name("Info")]
    [Description("Commands for delivering information about the bot")]
    public class Info : DiscordModuleBase<HanekawaContext>
    {
        [Name("About")]
        [Command("about", "info", "bot", "botinfo")]
        [Description("General information about the bot and links")]
        [RequireBotGuildPermissions(Permission.EmbedLinks)]
        [Cooldown(1, 5, CooldownMeasure.Seconds, HanaCooldown.Whatever)]
        [RequiredChannel]
        public async Task AboutAsync()
        {
            var appData = await Context.Bot.GetCurrentApplicationAsync();

            var embed = new LocalEmbedBuilder
            {
                Author = new LocalEmbedAuthorBuilder { Name = appData.Name, IconUrl = appData.IconHash },
                Fields =
                {
                    new LocalEmbedFieldBuilder { Name = "Up Time", Value = (DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize(3), IsInline = true },
                    new LocalEmbedFieldBuilder { Name = "Support", Value = "[Invite Link](https://discord.gg/gGu5TT6)", IsInline = true },
                    new LocalEmbedFieldBuilder { Name = "Bot Invite", Value = "[Invite Link](https://discordapp.com/api/oauth2/authorize?client_id=431610594290827267&scope=bot&permissions=8)", IsInline = true }
                }
            }.Create(appData.Description, Context.Colour.Get(Context.Guild.Id));
            await Context.ReplyAsync(embed);
        }

        [Name("Up Time")]
        [Command("uptime")]
        [Description("Show bots up time")]
        [RequireBotGuildPermissions(Permission.EmbedLinks)]
        [Cooldown(1, 5, CooldownMeasure.Seconds, HanaCooldown.Whatever)]
        [RequiredChannel]
        public async Task UptimeAsync() 
            => await Context.ReplyAsync($"Current up time: {(DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize(3)}");
    }
}