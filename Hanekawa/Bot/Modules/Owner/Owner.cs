using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Administration;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Microsoft.Extensions.DependencyInjection;

using Qmmands;

namespace Hanekawa.Bot.Modules.Owner
{
    [Name("Owner")]
    [Description("Owner commands for bot overview")]
    [BotOwnerOnly]
    [RequireBotGuildPermissions(Permission.EmbedLinks)]
    public class Owner : HanekawaModule
    {
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
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
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
