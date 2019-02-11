using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Modules.Club.Handler;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

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
                    foreach (var x in clubs)
                    {
                        if (x.Rank == 1)
                        {
                            var toPromote = await db.ClubPlayers.FirstOrDefaultAsync(y =>
                                                y.GuildId == x.GuildId && y.ClubId == x.Id && y.Rank == 2) ??
                                            await db.ClubPlayers.FirstOrDefaultAsync(y =>
                                                y.GuildId == x.GuildId && y.ClubId == x.Id && y.Rank == 3);
                            toPromote.Rank = 1;
                        }   
                        db.ClubPlayers.Remove(x);
                    }
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        private Task ClubJoin(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel,
            SocketReaction reaction) =>
            ClubJoinLeave(msg, channel, reaction);

        private Task ClubLeave(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction) =>
            ClubJoinLeave(msg, channel, reaction);

        private Task ClubJoinLeave(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel chan,
            SocketReaction reaction)
        {
            var _ = Task.Run(async () =>
            {
                if (!(chan is ITextChannel channel)) return;
                if (!reaction.Emote.Equals(new Emoji("\u2714"))) return;
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfigAsync(channel.Guild);
                    if (!cfg.ClubAdvertisementChannel.HasValue) return;
                    if (cfg.ClubAdvertisementChannel.Value != channel.Id) return;

                    var club = await db.ClubInfos.FirstOrDefaultAsync(x =>
                        x.GuildId == channel.Guild.Id && x.AdMessage == reaction.MessageId);
                    if (club == null || !club.Public) return;

                    var clubUser = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                        x.UserId == reaction.UserId && x.GuildId == channel.GuildId && x.ClubId == club.Id);
                    if (clubUser == null)
                        await _management.AddUserAsync(db, reaction.User.GetValueOrDefault(), channel.Guild, club.Id,
                            cfg);
                    else
                        await _management.RemoveUserAsync(db, reaction.User.GetValueOrDefault(), channel.Guild, club.Id, cfg);
                }
            });
            return Task.CompletedTask;
        }
    }
}