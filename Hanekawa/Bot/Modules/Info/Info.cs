using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Disqord.Bot;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Humanizer;
using Qmmands;
using Cooldown = Hanekawa.Shared.Command.Cooldown;

namespace Hanekawa.Bot.Modules.Info
{
    [Name("Info")]
    [Description("Commands for delivering information about the bot")]
    public class Info : DiscordModuleBase<HanekawaContext>
    {
        [Name("About")]
        [Command("about", "info", "bot", "botinfo")]
        [Description("General information about the bot and links")]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        [Cooldown(1, 5, CooldownMeasure.Seconds, Cooldown.Whatever)]
        [RequiredChannel]
        public async Task AboutAsync()
        {
            var appData = await Context.Client.GetApplicationInfoAsync();

            var embed = new EmbedBuilder().Create(appData.Description, Context.Colour.Get(Context.Guild.Id));
            embed.Author = new EmbedAuthorBuilder{ Name = appData.Name, IconUrl = appData.IconUrl };
            embed.Fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder { Name = "Up Time", Value = (DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize(3), IsInline = true },
                new EmbedFieldBuilder { Name = "Support", Value = "[Invite Link](https://discord.gg/gGu5TT6)", IsInline = true },
                new EmbedFieldBuilder { Name = "Bot Invite", Value = "[Invite Link](https://discordapp.com/api/oauth2/authorize?client_id=431610594290827267&scope=bot&permissions=8)", IsInline = true }
            };
            await Context.ReplyAsync(embed);
        }

        [Name("Up Time")]
        [Command("uptime")]
        [Description("Show bots up time")]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        [Cooldown(1, 5, CooldownMeasure.Seconds, Cooldown.Whatever)]
        [RequiredChannel]
        public async Task UptimeAsync() 
            => await Context.ReplyAsync($"Current up time: {(DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize(3)}");
    }
}