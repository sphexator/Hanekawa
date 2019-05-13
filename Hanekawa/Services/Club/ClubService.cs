using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Modules.Club.Handler;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Services.Club
{
    public class ClubService : IHanaService, IRequiredService
    {
        private readonly DiscordSocketClient _client;
        private readonly Management _management;

        public ClubService(DiscordSocketClient client, Management management)
        {
            _client = client;
            _management = management;
            _client.ReactionAdded += ClubJoin;
            _client.ReactionRemoved += ClubLeave;
            _client.UserLeft += ClubRemoveAsync;
        }

        private Task ClubRemoveAsync(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var clubs = await db.ClubPlayers.Where(x => x.GuildId == user.Guild.Id && x.UserId == user.Id)
                        .ToListAsync();
                    if (clubs.Count == 0) return;
                    foreach (var x in clubs) await _management.RemoveUserAsync(db, user, x);
                }
            });
            return Task.CompletedTask;
        }

        private Task ClubJoin(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel,
            SocketReaction reaction) =>
            ClubJoinLeave(msg, channel, reaction);

        private Task ClubLeave(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel,
            SocketReaction reaction) =>
            ClubJoinLeave(msg, channel, reaction);

        private Task ClubJoinLeave(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel chan,
            SocketReaction reaction)
        {
            var _ = Task.Run(async () =>
            {
                if (!(chan is ITextChannel channel)) return;
                if (!reaction.Emote.Equals(new Emoji("\u2714"))) return;
                if (reaction.User.GetValueOrDefault().IsBot) return;
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
                    if (clubUser == null)
                        await _management.AddUserAsync(db, reaction.User.GetValueOrDefault(), channel.Guild, club.Id,
                            cfg);
                    else
                        await _management.RemoveUserAsync(db, reaction.User.GetValueOrDefault(), channel.Guild, club.Id,
                            cfg);
                }
            });
            return Task.CompletedTask;
        }
    }
}