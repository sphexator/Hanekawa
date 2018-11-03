using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Preconditions;
using Humanizer;

namespace Hanekawa.Modules.Help
{
    public class Info : InteractiveBase
    {
        [Command("bot info")]
        [Alias("botinfo")]
        [Summary("General info about the bot")]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task DmInfoPosTask()
        {
            await Context.Message.DeleteAsync();
            var application = await Context.Client.GetApplicationInfoAsync();
            var embed = new EmbedBuilder
            {
                Color = Color.Purple
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
                Value = application.Description
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
            embed.AddField("Support", "[link](https://discord.gg/9tq4xNT)", true);
            embed.AddField("Invite link",
                "[link](https://discordapp.com/api/oauth2/authorize?client_id=431610594290827267&scope=bot&permissions=8)",
                true);
            await (await Context.User.GetOrCreateDMChannelAsync()).SendMessageAsync(null, false, embed.Build());
        }

        [Command("bot info")]
        [Alias("botinfo")]
        [Summary("General info about the bot")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [Priority(1)]
        [RequiredChannel]
        public async Task InfoPosTask()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            var embed = new EmbedBuilder
            {
                Color = Color.Purple
            };
            var about = new EmbedFieldBuilder
            {
                IsInline = false,
                Name = "About",
                Value = application.Description
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
            embed.AddField("Support", "[link](https://discord.gg/9tq4xNT)", true);
            embed.AddField("Invite link",
                "[link](https://discordapp.com/api/oauth2/authorize?client_id=431610594290827267&scope=bot&permissions=8)",
                true);
            await ReplyAsync(null, false, embed.Build());
        }

        [Command("uptime")]
        [Summary("Display uptime of the bot")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task UptimeTask()
        {
            await ReplyAndDeleteAsync(null, false,
                new EmbedBuilder()
                    .Reply($"Bot uptime: {(DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize()}")
                    .Build(), TimeSpan.FromSeconds(20));
        }

        [Command("server info", RunMode = RunMode.Async)]
        [Alias("serverinfo", "sinfo")]
        [Summary("Displays information about the guild/server")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ServerInformation()
        {
            var embed = new EmbedBuilder
            {
                Color = Color.Purple,
                Author = new EmbedAuthorBuilder
                    {IconUrl = Context.Guild.IconUrl, Name = $"{Context.Guild.Name} ({Context.Guild.Id})"}
            };
            embed.AddField("Verification Level", Context.Guild.VerificationLevel, true);
            embed.AddField("Region", Context.Guild.VoiceRegionId, true);
            embed.AddField($"Members[{Context.Guild.MemberCount}]",
                $"{Context.Guild.Users.Count(x => x.Status != UserStatus.Offline && x.Status != UserStatus.Invisible)} online",
                true);
            embed.AddField("Channels", $"**Categories:** {Context.Guild.CategoryChannels.Count}\n" +
                                       $"**Text Channels:** {Context.Guild.TextChannels.Count}\n" +
                                       $"**Voice Channels:** {Context.Guild.VoiceChannels.Count}", true);
            embed.AddField("Server owner",
                $"{Context.Guild.Owner.Username}#{Context.Guild.Owner.Discriminator} ({Context.Guild.OwnerId})");
            embed.AddField("Created", $"{Context.Guild.CreatedAt.Humanize()} ({Context.Guild.CreatedAt})", true);
            embed.AddField($"Roles[{Context.Guild.Roles.Count}]",
                "Use [prefix]server roles to view all roles on the server.", true);
            await ReplyAsync(null, false, embed.Build());
        }

        [Command("server roles", RunMode = RunMode.Async)]
        [Alias("serverroles", "sroles")]
        [Summary("Displays all roles in the guild/server")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ServerRoles()
        {
            var roles = Context.Guild.Roles.Aggregate<SocketRole, string>(null,
                (current, x) => current + $"{x.Name}, ");
            var embed = new EmbedBuilder
            {
                Color = Color.Purple,
                Author = new EmbedAuthorBuilder
                    {IconUrl = Context.Guild.IconUrl, Name = $"Roles for {Context.Guild.Name}"},
                Description = roles
            };
            await ReplyAsync(null, false, embed.Build());
        }

        [Command("latency", RunMode = RunMode.Async)]
        [Alias("ping", "pong", "rtt")]
        [Summary("Returns the current estimated round-trip latency over WebSocket and REST")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task LatencyAsync()
        {
            IUserMessage message = null;
            Stopwatch stopwatch = null;
            var heartbeat = Context.Client.Latency;
            message = await ReplyAsync($"hearbeat: {heartbeat}ms: init: ---, rtt: ---");

            var tcs = new TaskCompletionSource<long>();
            var timeout = Task.Delay(TimeSpan.FromSeconds(30));

            Task TestMessageAsync(SocketMessage arg)
            {
                if (arg.Id != message?.Id) return Task.CompletedTask;
                tcs.SetResult(stopwatch.ElapsedMilliseconds);
                return Task.CompletedTask;
            }

            stopwatch = Stopwatch.StartNew();
            var init = stopwatch.ElapsedMilliseconds;

            Context.Client.MessageReceived += TestMessageAsync;
            var task = await Task.WhenAny(tcs.Task, timeout);
            Context.Client.MessageReceived -= TestMessageAsync;
            stopwatch.Stop();

            if (task == timeout)
            {
                await message.ModifyAsync(x => x.Content = $"heartbeat: {heartbeat}ms, init: {init}ms, rtt: timed out");
            }
            else
            {
                var rtt = await tcs.Task;
                await message.ModifyAsync(x => x.Content = $"heartbeat: {heartbeat}ms, init: {init}ms, rtt: {rtt}ms");
            }
        }
    }
}