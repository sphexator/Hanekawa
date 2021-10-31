using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Hosting;
using Disqord.Rest;
using Hanekawa.Bot.Commands;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;

namespace Hanekawa.Bot.Service.Administration
{
    public class AutoAssignService : DiscordClientService
    {
        private readonly Hanekawa _bot;
        private readonly IServiceProvider _provider;
        private readonly Logger _logger;
        private readonly SemaphoreSlim _lock = new (1, 1);

        public AutoAssignService(ILogger<AutoAssignService> logger, Hanekawa client, IServiceProvider provider) : base(logger, client)
        {
            _provider = provider;
            _bot = client;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task<bool> PostAsync(HanekawaCommandContext context, ITextChannel channel, ChannelConfig cfg, DbService db)
        {
            var roles = await db.SelfAssignAbleRoles.Where(x => x.GuildId == context.Guild.Id)
                .OrderByDescending(x => x.Exclusive).ToListAsync();
            if (roles.Count == 0) return false;
            db.SelfAssignReactionRoles.RemoveRange(cfg.AssignReactionRoles);
            cfg.AssignReactionRoles = new List<SelfAssignReactionRole>();
            await db.SaveChangesAsync();
            var nonExclusiveRoles = roles.Where(x => !x.Exclusive).ToList();
            var exclusiveRoles = roles.Where(x => x.Exclusive).ToList();
            for (var i = 0; i < nonExclusiveRoles.Count;)
            {
                var strings = new List<string>();
                var emoteStrings = new List<string>();
                var emotes = new List<ICustomEmoji>();
                for (var j = 0; j < 20; j++)
                {
                    if (i >= nonExclusiveRoles.Count) continue;
                    var x = nonExclusiveRoles[i];
                    if (!context.Guild.Roles.TryGetValue(x.RoleId, out var role)) continue;
                    strings.Add(LocalCustomEmoji.TryParse(x.EmoteMessageFormat, out var result)
                        ? $"{result}{role.Mention}"
                        : $"{role.Mention}");
                    if (result != null)
                    {
                        emotes.Add(result);
                        emoteStrings.Add(result.GetMessageFormat());
                    }

                    i++;
                }

                var message =
                    await channel.SendMessageAsync(
                        new LocalMessage().Create(ReactionEmbed(strings, context.Guild.Id)));
                foreach (var x in emotes)
                {
                    await message.AddReactionAsync(LocalEmoji.FromEmoji(x));
                }

                cfg.AssignReactionRoles.Add(new SelfAssignReactionRole
                {
                    Exclusive = false,
                    ChannelId = channel.Id,
                    GuildId = context.Guild.Id,
                    MessageId = message.Id,
                    Reactions = emoteStrings,
                    ConfigId = cfg.GuildId,
                    Config = cfg
                });
            }

            for (var i = 0; i < exclusiveRoles.Count;)
            {
                var strings = new List<string>();
                var emoteStrings = new List<string>();
                var emotes = new List<ICustomEmoji>();
                for (var j = 0; j < 20; j++)
                {
                    if (i >= exclusiveRoles.Count) continue;
                    var x = exclusiveRoles[i];
                    if (!context.Guild.Roles.TryGetValue(x.RoleId, out var role)) continue;
                    strings.Add(LocalCustomEmoji.TryParse(x.EmoteMessageFormat, out var result)
                        ? $"{result}{role.Mention}"
                        : $"{role.Mention}");
                    if (result != null)
                    {
                        emotes.Add(result);
                        emoteStrings.Add(result.GetMessageFormat());
                    }

                    i++;
                }

                var message =
                    await channel.SendMessageAsync(
                        new LocalMessage().Create(ReactionEmbed(strings, context.Guild.Id)));
                foreach (var x in emotes)
                {
                    await message.AddReactionAsync(LocalEmoji.FromEmoji(x));
                }
                
                cfg.AssignReactionRoles.Add(new SelfAssignReactionRole
                {
                    Exclusive = true,
                    ChannelId = channel.Id,
                    GuildId = context.Guild.Id,
                    MessageId = message.Id,
                    Reactions = emoteStrings,
                    ConfigId = cfg.GuildId,
                    Config = cfg
                });
            }

            await db.SaveChangesAsync();
            return true;
        }

        public LocalEmbed ReactionEmbed(IEnumerable<string> strings, Snowflake guildId)
        {
            var str = new StringBuilder();
            var counter = 0;
            foreach (var x in strings)
            {
                counter = String(counter, str, x);
            }
            
            var embed = new LocalEmbed
            {
                Title = "Self-assignable roles",
                Description = str.ToString(),
                Color = _provider.GetRequiredService<CacheService>().GetColor(guildId)
            };
            return embed;
        }

        private static int String(int counter, StringBuilder str, string x)
        {
            if (counter == 0) str.AppendLine($"{x}");
            if (counter.IsDivisible(5)) str.AppendLine($"{x}");
            else str.Append($"- {x}");

            if (counter == 5) counter = 0;
            else counter++;
            return counter;
        }

        protected override async ValueTask OnReactionAdded(ReactionAddedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            if (e.Member is null) return;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateEntityAsync<ChannelConfig>(e.GuildId.Value);
            if (!cfg.SelfAssignableChannel.HasValue) return;
            if (e.ChannelId != cfg.SelfAssignableChannel.Value) return;
            var reactMessage = cfg.AssignReactionRoles.FirstOrDefault(x => x.MessageId == e.MessageId);
            if (reactMessage != null) return;
            
            var reaction = await db.SelfAssignAbleRoles.FirstOrDefaultAsync(x =>
                x.GuildId == e.GuildId.Value && x.EmoteMessageFormat == e.Emoji.GetMessageFormat());
            if (reaction == null) return;
            var guild = e.Member.GetGuild();
            if (!guild.Roles.TryGetValue(reaction.RoleId, out var role)) return;
            await _lock.WaitAsync();
            try
            {
                var userRoles = e.Member.GetRoles();
                var userRolesModified = userRoles.Keys.ToList();
                if (reaction.Exclusive)
                {
                    var roles = await db.SelfAssignAbleRoles.Where(x => x.GuildId == e.GuildId.Value && x.Exclusive)
                        .ToListAsync();
                    foreach (var x in roles)
                    {
                        if (!guild.Roles.TryGetValue(x.RoleId, out var exclusiveRole)) continue;
                        if (e.Member.GetRoles().ContainsKey(exclusiveRole.Id))
                            userRolesModified.Remove(exclusiveRole.Id);
                    }
                }

                if (!userRoles.ContainsKey(role.Id)) userRolesModified.Add(role.Id);
                if (userRoles.Count != userRolesModified.Count)
                    await e.Member.ModifyAsync(x => x.RoleIds = userRolesModified);
            }
            catch (Exception exception)
            {
                _logger.Error(exception,
                    $"Couldn't modify roles for {e.UserId} in {e.GuildId.Value} for emote {e.Emoji}");
            }
            finally
            {
                _lock.Release();
            }
        }

        protected override async ValueTask OnReactionRemoved(ReactionRemovedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateEntityAsync<ChannelConfig>(e.GuildId.Value);
            if (!cfg.SelfAssignableChannel.HasValue) return;
            if (e.ChannelId != cfg.SelfAssignableChannel.Value) return;
            var reactMessage = cfg.AssignReactionRoles.FirstOrDefault(x => x.MessageId == e.MessageId);
            if (reactMessage != null) return;
            
            var reaction = await db.SelfAssignAbleRoles.FirstOrDefaultAsync(x =>
                x.GuildId == e.GuildId.Value && x.EmoteMessageFormat == e.Emoji.GetMessageFormat());
            if (reaction == null) return;
            if (!_bot.GetGuild(e.GuildId.Value).Roles.TryGetValue(reaction.RoleId, out var role)) return;
            await _lock.WaitAsync();
            try
            {
                var user = await _bot.GetOrFetchMemberAsync(e.GuildId.Value, e.UserId);
                var roles = user.GetRoles();
                if (roles.ContainsKey(role.Id)) await user.RevokeRoleAsync(role.Id);
            }
            catch (Exception exception)
            {
                _logger.Error(exception,
                    $"Couldn't modify roles for {e.UserId} in {e.GuildId.Value} for emote {e.Emoji}");
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}