using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Preconditions;
using Humanizer;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Help
{
    public class Info : InteractiveBase
    {
        [Command("bot info")]
        [Alias("botinfo", "bot")]
        [Summary("General info about the bot")]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task DmInfoPosTask()
        {
            await Context.Message.DeleteAsync();
            var application = await Context.Client.GetApplicationInfoAsync();
            var currentProcess = Process.GetCurrentProcess();
            var embed = new EmbedBuilder().CreateDefault(Context.Guild.Id);
            embed.AddField("Instance owned by", $"{application.Owner.Username}#{application.Owner.Discriminator}", true);
            embed.AddField("Creator", "[Sphexator](https://github.com/sphexator)", true);
            embed.AddField("About", application.Description, true);
            embed.AddField("Up time", $"{(DateTime.Now - currentProcess.StartTime).Humanize()}", true);
            embed.AddField("Support", "[link](https://discord.gg/gGu5TT6)", true);
            embed.AddField("Invite link",
                "[link](https://discordapp.com/api/oauth2/authorize?client_id=431610594290827267&scope=bot&permissions=8)",
                true);
            await (await Context.User.GetOrCreateDMChannelAsync()).SendMessageAsync(null, false, embed.Build());
        }

        [Command("bot info")]
        [Alias("botinfo", "bot")]
        [Summary("General info about the bot")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [Priority(1)]
        [RequiredChannel]
        public async Task InfoPosTask()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            var currentProcess = Process.GetCurrentProcess();
            var embed = new EmbedBuilder()
                .CreateDefault(Context.Guild.Id)
                .AddField("Instance owned by", $"{application.Owner.Username}#{application.Owner.Discriminator}", true)
                .AddField("Creator", "[Sphexator](https://github.com/sphexator)", true)
                .AddField("About", application.Description, true)
                .AddField("Up time", $"{(DateTime.Now - currentProcess.StartTime).Humanize()}", true)
                .AddField("Memory", $"{currentProcess.WorkingSet64.SizeSuffix()} (Peak {currentProcess.PeakWorkingSet64.SizeSuffix()})")
                .AddField("Support", "[link](https://discord.gg/gGu5TT6)", true)
                .AddField("Invite link",
                    "[link](https://discordapp.com/api/oauth2/authorize?client_id=431610594290827267&scope=bot&permissions=8)",
                    true);
            await Context.ReplyAsync(embed);
        }

        [Command("uptime")]
        [Summary("Display uptime of the bot")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task UptimeTask()
        {
            await ReplyAndDeleteAsync(null, false,
                new EmbedBuilder()
                    .CreateDefault($"Bot uptime: {(DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize()}", Context.Guild.Id)
                    .Build(), TimeSpan.FromSeconds(20));
        }

        [Command("server info", RunMode = RunMode.Async)]
        [Alias("serverinfo", "sinfo", "si", "server")]
        [Summary("Displays information about the guild/server")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ServerInformation()
        {
            var embed = new EmbedBuilder()
                .CreateDefault(Context.Guild.Id)
                .WithAuthor(new EmbedAuthorBuilder
                    {IconUrl = Context.Guild.IconUrl, Name = $"{Context.Guild.Name} ({Context.Guild.Id})"});

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
        [Alias("serverroles", "sroles", "sr")]
        [Summary("Displays all roles in the guild/server")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ServerRoles()
        {
            var roles = Context.Guild.Roles.Aggregate<SocketRole, string>(null,
                (current, x) => current + $"{x.Name}, ");
            var embed = new EmbedBuilder()
                .CreateDefault(roles, Context.Guild.Id)
                .WithAuthor(new EmbedAuthorBuilder
                    {IconUrl = Context.Guild.IconUrl, Name = $"Roles for {Context.Guild.Name}"});
            await Context.ReplyAsync(embed);
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