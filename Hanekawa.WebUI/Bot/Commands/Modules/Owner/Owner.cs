using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Api;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Models;
using Disqord.Rest;
using Hanekawa.Entities.Administration;
using Hanekawa.Infrastructure;
using Hanekawa.Entities.Color;
using Hanekawa.WebUI.Extensions;
using Hanekawa.WebUI.Bot.Service.Cache;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.WebUI.Bot.Commands.Modules.Owner
{
    [Name("Owner")]
    [Description("Owner commands")]
    [RequireAuthor(111123736660324352)]
    public class Owner : HanekawaCommandModule
    {
        [Command("mmlol")]
        [RequireGuild(431617676859932704)]
        public async Task ReturnRole()
        {
            try
            {
                await Context.Message.DeleteAsync();
                Context.Guild.Roles.TryGetValue(431621144517279755, out var role);
                if (role != null) await Context.Author.GrantRoleAsync(role.Id);
            }
            catch
            {
                await Context.Message.DeleteAsync();
                var roles = await Context.Guild.FetchRolesAsync();
                var role = roles.FirstOrDefault(x => x.Id == 431621144517279755);
                if (role != null) await Context.Author.GrantRoleAsync(role.Id);
            }
        }
        
        [Name("Servers")]
        [Command("servers")]
        [Description("List all servers bot is part of")]
        public DiscordMenuCommandResult ServersAsync()
        {
            var servers = new List<string>();
            var totalMembers = 0;
            var guilds = Context.Bot.GetGuilds();
            foreach (var (_, value) in Context.Bot.GetGuilds())
            {
                try
                {
                    totalMembers += value.MemberCount;
                    var sb = new StringBuilder();
                    sb.AppendLine($"Server: {value.Name} ({value.Id})");
                    sb.AppendLine(value.MaxMemberCount != null
                        ? $"Members: {value.MaxMemberCount.Value}"
                        : $"Members: {value.MemberCount}");
                    sb.AppendLine($"Owner: {Context.Bot.GetMember(value.Id, value.OwnerId)}");
                    servers.Add(sb.ToString());
                }
                catch { /* IGNORE */}
            }

            return Pages(servers.Pagination(
                Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId),
                Context.Bot.CurrentUser.GetAvatarUrl(), $"Server Count: {guilds.Count} | Member Count: {totalMembers}"));
        }
        
        [Name("Blacklist")]
        [Command("blacklist")]
        [Description("Blacklists a server for the bot to join")]
        public async Task<DiscordCommandResult> BlacklistAsync(Snowflake guildId, string reason = null)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var blacklist = await db.Blacklists.FindAsync(guildId);
            if (blacklist == null)
            {
                await db.Blacklists.AddAsync(new Blacklist
                {
                    GuildId = guildId,
                    ResponsibleUser = Context.Author.Id,
                    Reason = reason
                });
                await db.SaveChangesAsync();
                return Reply($"Added blacklist on {guildId}!", HanaBaseColor.Ok());
            }

            blacklist.Unban = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Reply($"Removed blacklist on {guildId}!", HanaBaseColor.Ok());
        }

        [Name("Sudo")]
        [Command("sudo")]
        [Description("Execute commands as a different user")]
        public async Task SudoAsync(Snowflake userId, [Remainder] string command)
        {
            var author = Context.Bot.GetMember(Context.GuildId, userId) ??
                         await Context.Bot.FetchMemberAsync(Context.GuildId, userId);
            var message = SudoGatewayUserMessage(Context.Message, command, author);
            Context.Bot.Queue.Post(
                new HanekawaCommandContext(Context.Bot, Context.Prefix, command, message, Context.Channel,
                    Context.Scope), async e => await e.ContinueAsync());
        }

        private static IGatewayUserMessage SudoGatewayUserMessage(IMessage message, string input, IMember author)
        {
            var roles = author.GetRoles();
            var roleList = new HashSet<Snowflake>();
            foreach (var xRole in roles)
            {
                if (!roleList.TryGetValue(xRole.Key, out _)) roleList.Add(xRole.Key);
            }
            var userModel = new UserJsonModel
            {
                Avatar = author.AvatarHash,
                Discriminator = Convert.ToInt16(author.Discriminator),
                Id = author.Id,
                Username = author.Name,
                Bot = author.IsBot,
                PublicFlags = author.PublicFlags,
            };
            return new TransientGatewayUserMessage(message.Client, new MessageJsonModel
            {
                Id = message.Id,
                Content = input,
                Author = userModel,
                Timestamp = message.CreatedAt(),
                Pinned = false,
                Embeds = null,
                Tts = false,
                GuildId = author.GuildId,
                ChannelId = message.ChannelId,
                Member = new Optional<MemberJsonModel>(new MemberJsonModel
                {
                    Nick = author.Nick,
                    Roles = roleList.ToArray(),
                    Permissions = new Optional<ulong>(author.GetPermissions()),
                    JoinedAt = author.JoinedAt,
                    Deaf = author.IsDeafened,
                    Mute = author.IsMuted,
                    Pending = author.IsPending,
                    PremiumSince = author.BoostedAt,
                    User = userModel
                }),
                EditedTimestamp = null,
                Type = MessageType.Default
            });
        }
    }
}