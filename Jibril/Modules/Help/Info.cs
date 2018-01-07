using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Humanizer;
using Jibril.Data.Variables;
using Jibril.Services.Common;

namespace Jibril.Modules.Help
{
    public class Info : InteractiveBase
    {
        [Command("info")]
        [Summary("General info about the bot")]
        public async Task InfoPosTask()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            var embed = new EmbedBuilder
            {
                Color = new Color(Colours.DefaultColour)
            };
            var host = new EmbedFieldBuilder
            {
                IsInline = true,
                Name = "Instance owned by",
                Value = $"{application.Owner.Mention}"
            };
            var creator = new EmbedFieldBuilder
            {
                IsInline = true,
                Name = "Creator",
                Value = "[Sphexator](https://github.com/sphexator)"
            };
            var about = new EmbedFieldBuilder
            {
                IsInline = false,
                Name = "About",
                Value = "Jibril is a discord bot focused on rewarding user interactivity while keeping it clean with several services analyzing user behaviours.\n" +
                        "Jibril is made in C# and is not open source."
            };
            var currentProcess = Process.GetCurrentProcess();
            var uptime = new EmbedFieldBuilder
            {
                IsInline = false,
                Name = "Uptime",
                Value = $"{(DateTime.Now - currentProcess.StartTime).Humanize()}"
            };
            embed.AddField(host);
            embed.AddField(creator);
            embed.AddField(about);
            embed.AddField(uptime);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("uptime")]
        [Summary("Display uptime of bot")]
        public async Task UptimeTask()
        {
            var currentProcess = Process.GetCurrentProcess();
            var embed = EmbedGenerator.DefaultEmbed($"{(DateTime.Now - currentProcess.StartTime).Humanize()}",
                Colours.DefaultColour);
            await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(20));
        }
    }
}
