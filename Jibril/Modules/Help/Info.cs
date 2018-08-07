﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Extensions;
using Hanekawa.Preconditions;
using Humanizer;

namespace Hanekawa.Modules.Help
{
    public class Info : InteractiveBase
    {
        [Command("info")]
        [Summary("General info about the bot")]
        [RequiredChannel(339383206669320192)]
        public async Task InfoPosTask()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            var embed = new EmbedBuilder
            {
                Color = Color.DarkPurple
            };
            var host = new EmbedFieldBuilder
            {
                IsInline = true,
                Name = "Instance owned by",
                Value = $"{application.Owner.Username}#{application.Owner.Discriminator}"
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
                Value =
                    "Jibril is a discord bot focused on rewarding user interactivity while keeping it clean with several services analyzing user behaviours.\n" +
                    "Jibril is made in C# using the Discord.Net library and is not open source."
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
            await ReplyAsync(null, false, embed.Build());
        }

        [Command("uptime")]
        [Summary("Display uptime of bot")]
        public async Task UptimeTask()
        {
            await ReplyAndDeleteAsync(null, false,
                new EmbedBuilder().Reply($"Bot uptime: {(DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize()}")
                    .Build(), TimeSpan.FromSeconds(20));
        }
    }
}