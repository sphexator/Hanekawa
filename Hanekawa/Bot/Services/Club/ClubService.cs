using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Club
{
    public partial class ClubService : INService, IRequired
    {
        private readonly OverwritePermissions _allowOverwrite =
            new OverwritePermissions(addReactions: PermValue.Allow, sendMessages: PermValue.Allow,
                attachFiles: PermValue.Allow, embedLinks: PermValue.Allow, viewChannel: PermValue.Allow);

        private readonly DiscordSocketClient _client;

        private readonly OverwritePermissions _denyOverwrite = new OverwritePermissions(
            addReactions: PermValue.Deny, sendMessages: PermValue.Deny, attachFiles: PermValue.Deny,
            embedLinks: PermValue.Deny, viewChannel: PermValue.Deny);

        private readonly InternalLogService _log;
        private readonly Random _random;
        private readonly IServiceProvider _provider;

        public ClubService(DiscordSocketClient client, Random random, InternalLogService log, IServiceProvider provider)
        {
            _client = client;
            _random = random;
            _log = log;
            _provider = provider;

            _client.ReactionAdded += ClubReactionAdded;
            _client.ReactionRemoved += ClubReactionRemoved;
            _client.UserLeft += ClubUserLeft;
        }

        private Task ClubUserLeft(SocketGuildUser user)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using (var db = _provider.GetRequiredService<DbService>())
                    {
                        var clubs = await db.ClubPlayers.Where(x => x.GuildId == user.Guild.Id && x.UserId == user.Id)
                            .ToListAsync();
                        if (clubs.Count == 0) return;
                        var cfg = await db.GetOrCreateClubConfigAsync(user.Guild);
                        foreach (var x in clubs) await RemoveUserAsync(user, x.Id, db, cfg);
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Club Service) Error in {user.Guild.Id} for User Left- {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task ClubReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction) =>
            ClubJoinLeave(message, channel, reaction);

        private Task ClubReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction) =>
            ClubJoinLeave(message, channel, reaction);

        private Task ClubJoinLeave(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel chan,
            SocketReaction reaction)
        {
            _ = Task.Run(async () =>
            {
                if (!(chan is ITextChannel channel)) return;
                if (!reaction.Emote.Equals(new Emoji("\u2714"))) return;
                if (!(reaction.User.GetValueOrDefault() is SocketGuildUser user)) return;
                if (user.IsBot) return;
                try
                {
                    using (var db = _provider.GetRequiredService<DbService>())
                    {
                        var cfg = await db.GetOrCreateClubConfigAsync(channel.Guild);
                        if (!cfg.AdvertisementChannel.HasValue) return;
                        if (cfg.AdvertisementChannel.Value != channel.Id) return;

                        var club = await db.ClubInfos.FirstOrDefaultAsync(x =>
                            x.GuildId == channel.Guild.Id && x.AdMessage == reaction.MessageId);
                        if (club == null || !club.Public) return;

                        var clubUser = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                            x.UserId == reaction.UserId && x.GuildId == channel.GuildId && x.ClubId == club.Id);
                        if (clubUser == null) await AddUserAsync(user, club.Id, db, cfg);
                        else await RemoveUserAsync(user, club.Id, db, cfg);
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Club Service) Error in {user.Guild.Id} for Reaction added or removed - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}