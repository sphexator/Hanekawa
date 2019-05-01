using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Core.Interfaces;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Services.Club
{
    public partial class ClubService : INService, IRequired
    {
        private readonly OverwritePermissions _allowOverwrite =
            new OverwritePermissions(addReactions: PermValue.Allow, sendMessages: PermValue.Allow,
                attachFiles: PermValue.Allow, embedLinks: PermValue.Allow, viewChannel: PermValue.Allow);

        private readonly OverwritePermissions _denyOverwrite = new OverwritePermissions(
            addReactions: PermValue.Deny, sendMessages: PermValue.Deny, attachFiles: PermValue.Deny,
            embedLinks: PermValue.Deny, viewChannel: PermValue.Deny);

        private readonly DiscordSocketClient _client;
        private readonly Random _random;

        public ClubService(DiscordSocketClient client, Random random)
        {
            _client = client;
            _random = random;

            _client.ReactionAdded += _client_ReactionAdded;
            _client.ReactionRemoved += _client_ReactionRemoved;
            _client.UserLeft += _client_UserLeft;
        }

        private Task _client_UserLeft(SocketGuildUser user)
        {
            _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var clubs = await db.ClubPlayers.Where(x => x.GuildId == user.Guild.Id && x.UserId == user.Id)
                        .ToListAsync();
                    if (clubs.Count == 0) return;
                    var cfg = await db.GetOrCreateClubConfigAsync(user.Guild);
                    foreach (var x in clubs) await RemoveUserAsync(user, x.Id, db, cfg);
                }
            });
            return Task.CompletedTask;
        }

        private Task _client_ReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction) =>
            ClubJoinLeave(message, channel, reaction);

        private Task _client_ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
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
                using (var db = new DbService())
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
            });
            return Task.CompletedTask;
        }
    }
}