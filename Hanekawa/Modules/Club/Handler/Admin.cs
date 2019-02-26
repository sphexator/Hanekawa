using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Club;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions.Embed;
using Microsoft.EntityFrameworkCore;
using Quartz.Util;
using System;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Club.Handler
{
    public class Admin : IHanaService
    {
        private readonly Management _management;

        public Admin(Management management) => _management = management;

        public async Task CreateAsync(ICommandContext context, string name)
        {
            if (name.IsNullOrWhiteSpace()) return;
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(context.User as SocketGuildUser);
                var cfg = await db.GetOrCreateClubConfigAsync(context.Guild);
                if (userdata.Level < cfg.ChannelRequiredLevel)
                {
                    await context.ReplyAsync($"You do not meet the requirement to make a club (Level {cfg.ChannelRequiredLevel}).",
                        Color.Red.RawValue);
                    return;
                }

                var leaderCheck = await db.IsClubLeader(context.Guild.Id, context.User.Id);
                if (leaderCheck != null)
                {
                    await context.ReplyAsync("You're already a leader of a club, you can't create multiple clubs.",
                        Color.Red.RawValue);
                    return;
                }

                var club = await db.CreateClub(context.User, context.Guild, name, DateTimeOffset.UtcNow);
                var data = new ClubUser
                {
                    ClubId = club.Id,
                    GuildId = context.Guild.Id,
                    JoinDate = DateTimeOffset.UtcNow,
                    Rank = 1,
                    UserId = context.User.Id,
                    Id = await db.ClubPlayers.CountAsync() + 1
                };
                await db.ClubPlayers.AddAsync(data);
                await db.SaveChangesAsync();
                await context.ReplyAsync($"Successfully created club {name} !", Color.Green.RawValue);
            }
        }

        public async Task DemoteAsync(ICommandContext context, IGuildUser user)
        {
            if (context.User == user) return;
            using (var db = new DbService())
            {
                var club = await db.IsClubLeader(context.Guild.Id, context.User.Id);
                if (club == null) return;
                var toDemote = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                    x.ClubId == club.Id && x.GuildId == user.GuildId && x.UserId == user.Id);
                if (toDemote == null)
                {
                    await context.ReplyAsync($"Can't demote {user.Mention} because he/she is not part of {club.Name}",
                        Color.Red.RawValue);
                    return;
                }

                if (toDemote.Rank == 3)
                {
                    await context.ReplyAsync($"Cannot demote {user.Mention} any further. (Already lowest rank)");
                    return;
                }

                if (toDemote.Rank == 1) return;
                await _management.DemoteUserAsync(db, user, toDemote, club);
                await context.ReplyAsync($"Demoted {user.Mention} down to rank 3 in {club.Name}",
                    Color.Green.RawValue);
            }
        }
    }
}