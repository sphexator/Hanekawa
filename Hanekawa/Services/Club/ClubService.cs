using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Club;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions;
using Hanekawa.Modules.Club.Handler;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hanekawa.Services.Club
{
    public class ClubService : IHanaService, IRequiredService
    {
        private readonly OverwritePermissions _denyOverwrite = new OverwritePermissions(
            addReactions: PermValue.Deny, sendMessages: PermValue.Deny, attachFiles: PermValue.Deny,
            embedLinks: PermValue.Deny, viewChannel: PermValue.Deny);

        private readonly OverwritePermissions _allowOverwrite =
            new OverwritePermissions(addReactions: PermValue.Allow, sendMessages: PermValue.Allow,
                attachFiles: PermValue.Allow, embedLinks: PermValue.Allow, viewChannel: PermValue.Allow);

        private readonly DiscordSocketClient _client;
        private readonly Management _management;

        public ClubService(DiscordSocketClient client, Management management)
        {
            _client = client;
            _management = management;

            _client.ReactionRemoved += ClubJoinLeave;
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

                    var clubuser = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                        x.GuildId == channel.Guild.Id && x.ClubId == club.Id && x.UserId == reaction.UserId);
                    if (clubuser == null) return;

                    var user = reaction.User.Value as IGuildUser;
                    if (!reaction.User.IsSpecified) user = await channel.Guild.GetUserAsync(reaction.UserId);
                    await user.RemoveRoleAsync(channel.Guild.GetRole(club.Role.Value));

                    db.ClubPlayers.Remove(clubuser);
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        private static Task AddClubMemberAsync(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2,
            SocketReaction arg3)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    if (!(arg2 is ITextChannel channel)) return;
                    var cfg = await db.GetOrCreateGuildConfigAsync(channel.Guild);
                    if (!cfg.ClubAdvertisementChannel.HasValue) return;
                    if (cfg.ClubAdvertisementChannel.Value != channel.Id) return;

                    var club = await db.ClubInfos.FirstOrDefaultAsync(x =>
                        x.GuildId == channel.Guild.Id && x.AdMessage == arg3.MessageId);
                    if (!club.Public || club.AdMessage == null) return;

                    var Clubuser = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                        x.GuildId == channel.Guild.Id && x.ClubId == club.Id && x.UserId == arg3.UserId);
                    if (Clubuser != null) return;

                    var user = arg3.User.Value as IGuildUser;
                    if (!arg3.User.IsSpecified) user = await channel.Guild.GetUserAsync(arg3.UserId);
                    await user.AddRoleAsync(channel.Guild.GetRole(club.Role.Value));

                    var data = new ClubUser
                    {
                        ClubId = club.Id,
                        GuildId = channel.Guild.Id,
                        JoinDate = DateTimeOffset.UtcNow,
                        Rank = 3,
                        UserId = user.Id,
                        Id = await db.ClubPlayers.CountAsync(x => x.GuildId == channel.Guild.Id) + 1
                    };
                    db.ClubPlayers.Add(data);
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        public async Task CreateChannelAsync(DbService db, IGuild guild, GuildConfig cfg, string clubName,
            IGuildUser leader,
            IEnumerable<ClubUser> users, ClubInformation club)
        {
            ITextChannel channel;
            if (cfg.ClubChannelCategory.HasValue) channel = await guild.CreateTextChannelAsync(clubName, x => x.CategoryId = cfg.ClubChannelCategory.Value);
            else channel = await guild.CreateTextChannelAsync(clubName);
            await channel.AddPermissionOverwriteAsync(guild.EveryoneRole, _denyOverwrite);
            club.Channel = channel.Id;
            if (cfg.ClubRole)
            {
                var role = await guild.CreateRoleAsync(clubName, GuildPermissions.All);
                await channel.AddPermissionOverwriteAsync(role, _allowOverwrite);
                club.Role = role.Id;
                foreach (var x in users)
                {
                    try
                    {
                        await (await guild.GetUserAsync(x.UserId)).TryAddRoleAsync(role).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        //TODO Add logging
                    }
                }
            }
            else
            {
                foreach (var x in users)
                {
                    try
                    {
                        await channel.AddPermissionOverwriteAsync(await guild.GetUserAsync(x.UserId), _allowOverwrite);
                    }
                    catch (Exception e)
                    {
                        //TODO Add logging
                    }
                }
            }

            await db.SaveChangesAsync();
        }
    }
}