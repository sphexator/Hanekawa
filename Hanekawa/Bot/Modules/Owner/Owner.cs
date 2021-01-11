using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Services.ImageGen;
using Hanekawa.Bot.Services.Welcome;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Administration;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Qmmands;

namespace Hanekawa.Bot.Modules.Owner
{
    [Name("Owner")]
    [Description("Owner commands for bot overview")]
    [RequireUser(111123736660324352)]
    [RequireBotGuildPermissions(Permission.EmbedLinks)]
    public class Owner : HanekawaCommandModule
    {
        private readonly ImageGenerator _welcome;
        public Owner(ImageGenerator welcome) => _welcome = welcome;

        [Command("test")]
        public async Task TestAsync(string image, int aviSize = 60, int aviX = 10, int aviY = 10, int textSize = 33, int textX = 245, int textY = 40)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var banner = await _welcome.WelcomeBuilder(Context.Member, image, aviSize, aviX, aviY, textSize, textX, textY, true);
            banner.Position = 0;
            await Context.Channel.SendMessageAsync(new LocalAttachment(banner, "Welcome.gif"), "Test message", false, null, LocalMentions.None);
        }

        [Command("pfp")]
        public async Task PfpAsync()
        {
            var embed = new LocalEmbedBuilder
            {
                Description = "test",
                ImageUrl = Context.User.GetAvatarUrl()
            };
            await Context.Channel.SendMessageAsync("test avi", false, embed.Build());
        }

        [Name("Re-index Server Rankings")]
        [Command("rankindex")]
        [Description("Re-indexes the ranks, puts people that's left the server as inactive if they arnt already")]
        [Disabled]
        public async Task ReindexAsync()
        {
            // Ignore
        }

        [Name("Servers")]
        [Command("servers")]
        [Description("List all servers bot is part of")]
        public async Task ServersAsync()
        {
            var servers = new List<string>();
            var totalMembers = 0;
            foreach (var x in Context.Bot.Guilds)
            {
                totalMembers += x.Value.MemberCount;
                var sb = new StringBuilder();
                sb.AppendLine($"Server: {x.Value.Name} ({x.Value.Id.RawValue})");
                sb.AppendLine($"Members: {x.Value.MemberCount}");
                sb.AppendLine($"Owner: {x.Value.Owner.Mention}");
                servers.Add(sb.ToString());
            }
            await Context.PaginatedReply(servers, Context.Guild,null, $"Total Servers: {Context.Bot.Guilds.Count}\n " +
                                                                                                 $"Total Members: {totalMembers}");
        }

        [Name("Blacklist")]
        [Command("blacklist")]
        [Description("Blacklists a server for the bot to join")]
        public async Task BlacklistAsync(ulong guildId, string reason = null)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var blacklist = await db.Blacklists.FindAsync(guildId);
            if (blacklist == null)
            {
                await db.Blacklists.AddAsync(new Blacklist
                {
                    GuildId = guildId,
                    ResponsibleUser = Context.User.Id.RawValue,
                    Reason = reason
                });
                await db.SaveChangesAsync();
                await Context.ReplyAsync(new LocalEmbedBuilder().Create($"Added blacklist on {guildId}!", Color.Green));
                return;
            }

            blacklist.Unban = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            await Context.ReplyAsync(new LocalEmbedBuilder().Create($"Removed blacklist on {guildId}!", Color.Green));
        }
    }
}
