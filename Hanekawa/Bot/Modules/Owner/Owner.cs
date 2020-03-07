using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Administration;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Interactive;
using Qmmands;

namespace Hanekawa.Bot.Modules.Owner
{
    [Name("Owner")]
    [Description("Owner commands for bot overview")]
    [RequireOwner]
    public class Owner : InteractiveBase
    {
        [Name("Servers")]
        [Command("servers")]
        [Description("List all servers bot is part of")]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        public async Task ServersAsync()
        {
            var servers = new List<string>();
            foreach (var x in Context.Client.Guilds)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Server: {x.Name} ({x.Id})");
                sb.AppendLine($"Members: {x.MemberCount}");
                sb.AppendLine($"Owner: {x.OwnerId}");
                servers.Add(sb.ToString());
            }
            await PagedReplyAsync(servers.PaginateBuilder(Context.Guild, Context.User.GetName(), "Servers"));
        }

        [Name("Blacklist")]
        [Command("blacklist")]
        [Description("Blacklists a server for the bot to join")]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        public async Task BlacklistAsync(ulong guildId, string reason = null)
        {
            using var db = new DbService();
            var blacklist = await db.Blacklists.FindAsync(guildId);
            if (blacklist == null)
            {
                await db.Blacklists.AddAsync(new Blacklist
                {
                    GuildId = guildId,
                    ResponsibleUser = Context.User.Id,
                    Reason = reason
                });
                await db.SaveChangesAsync();
                await Context.ReplyAsync(new EmbedBuilder().Create($"Added blacklist on {guildId}!", Color.Green));
                return;
            }

            blacklist.Unban = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            await Context.ReplyAsync(new EmbedBuilder().Create($"Removed blacklist on {guildId}!", Color.Green));
        }
    }
}