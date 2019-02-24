using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Club;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Services.Logging;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz.Util;

namespace Hanekawa.Modules.Club.Handler
{
    public class Admin : InteractiveBase, IHanaService
    {
        private readonly OverwritePermissions _allowOverwrite =
            new OverwritePermissions(addReactions: PermValue.Allow,
                attachFiles: PermValue.Allow, embedLinks: PermValue.Allow, viewChannel: PermValue.Allow);

        private readonly OverwritePermissions _denyOverwrite = new OverwritePermissions(
            addReactions: PermValue.Deny, attachFiles: PermValue.Deny,
            embedLinks: PermValue.Deny, viewChannel: PermValue.Deny);

        private readonly Management _management;
        private readonly LogService _log;

        public Admin(Management management, LogService log)
        {
            _management = management;
            _log = log;
        }

        public async Task CreateAsync(ICommandContext context, string name)
        {
            if (name.IsNullOrWhiteSpace()) return;
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(context.User as SocketGuildUser);
                if (userdata.Level < 40)
                {
                    await context.ReplyAsync("You do not meet the requirement to make a club (Level 40).",
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

        public async Task AddAsync(ICommandContext context, IGuildUser user)
        {
            if (user == context.User) return;
            using (var db = new DbService())
            {
                var clubUser =
                    await db.ClubPlayers.FirstOrDefaultAsync(x =>
                        x.GuildId == context.Guild.Id && x.UserId == context.User.Id && x.Rank <= 2);
                if (clubUser == null) return;
                if (clubUser.Rank > 2)
                {
                    await context.ReplyAsync("You're not high enough rank to use that command!", Color.Red.RawValue);
                    return;
                }
                var check = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                    x.UserId == user.Id && x.GuildId == user.GuildId && x.ClubId == clubUser.Id);
                if (check != null)
                {
                    await context.ReplyAsync($"{user.Mention} is already a member of your club.");
                    return;
                }
                var clubData = await db.GetClubAsync(clubUser.ClubId, context.Guild);
                await ReplyAsync(
                    $"{user.Mention}, {context.User.Mention} has invited you to {clubData.Name}, do you accept? (y/n)");
                var status = true;
                while (status)
                    try
                    {
                        var response = await NextMessageAsync(new EnsureFromUserCriterion(user.Id),
                            TimeSpan.FromSeconds(30));

                        if (response.Content.ToLower() == "y") status = false;
                        if (response.Content.ToLower() == "n") return;
                    }
                    catch
                    {
                        await context.ReplyAsync("Invite expired.", Color.Red.RawValue);
                        return;
                    }
                await context.Channel.TriggerTypingAsync();
                await _management.AddUserAsync(db, user, clubUser);
                await context.ReplyAsync($"Added {user.Mention} to {clubData.Name}", Color.Green.RawValue);
            }
        }

        public async Task RemoveAsync(ICommandContext context, IGuildUser user)
        {
            if (user == Context.User) return;
            using (var db = new DbService())
            {
                var club = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                if (club == null) return;
                await context.Channel.TriggerTypingAsync();
                var response = await _management.RemoveUserAsync(db, user, club.Id);
                if (!response)
                {
                    await context.ReplyAsync($"Couldn't remove {user.Mention}", Color.Red.RawValue);
                    return;
                }

                await Context.ReplyAsync($"Removed {user.Mention} from {club.Name}", Color.Green.RawValue);
            }
        }

        public async Task CreateChannelAsync(ICommandContext context)
        {
            using (var db = new DbService())
            {
                var leader = await db.IsClubLeader(context.Guild.Id, context.User.Id);
                if (leader.Channel.HasValue) return;
                var cfg = await db.GetOrCreateClubConfigAsync(context.Guild);
                if (!cfg.ChannelCategory.HasValue)
                {
                    await Context.ReplyAsync("This server doesn\'t allow club channels", Color.Red.RawValue);
                    return;
                }

                var users = await db.ClubPlayers.Where(x => x.GuildId == Context.Guild.Id && x.ClubId == leader.Id)
                    .ToListAsync();
                var amount = await GetUsersOfLevelAsync(db, cfg.ChannelRequiredLevel, users);
                if (amount < cfg.ChannelRequiredAmount)
                {
                    await Context.ReplyAsync(
                        $"Club does not have the required amount({cfg.ChannelRequiredAmount}) of people that's of level {cfg.ChannelRequiredLevel} or higher to create a channel",
                        Color.Red.RawValue);
                    return;
                }

                var channel = await context.Guild.CreateTextChannelAsync(leader.Name,
                    x => x.CategoryId = cfg.ChannelCategory.Value);
                await channel.AddPermissionOverwriteAsync(context.Guild.EveryoneRole, _denyOverwrite);
                leader.Channel = channel.Id;
                if (cfg.RoleEnabled)
                {
                    var role = await context.Guild.CreateRoleAsync(leader.Name, GuildPermissions.All);
                    await channel.AddPermissionOverwriteAsync(role, _allowOverwrite);
                    leader.Role = role.Id;
                    foreach (var x in users)
                        try
                        {
                            await (await context.Guild.GetUserAsync(x.UserId)).TryAddRoleAsync(role)
                                .ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            _log.LogAction(LogLevel.Error, e.ToString(), "Club add user channel");
                        }
                }
                else
                {
                    foreach (var x in users)
                        try
                        {
                            await channel.AddPermissionOverwriteAsync(await context.Guild.GetUserAsync(x.UserId),
                                _allowOverwrite);
                        }
                        catch (Exception e)
                        {
                            _log.LogAction(LogLevel.Error, e.ToString(), "Club add user channel");
                        }
                }

                await db.SaveChangesAsync();
            }
        }

        public async Task LeaveAsync(ICommandContext context)
        {
            using (var db = new DbService())
            {
                var clubs = await db.ClubPlayers
                    .Where(x => x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id).ToListAsync();
                ClubUser club;
                switch (clubs.Count)
                {
                    case 0:
                        await context.ReplyAsync("You're currently not in a club.", Color.Red.RawValue);
                        return;
                    case 1:
                        club = clubs.FirstOrDefault();
                        await _management.RemoveUserAsync(db, context.User, context.Guild, club);
                        await context.ReplyAsync(
                            $"Successfully left {(await db.GetClubAsync(club.ClubId, Context.Guild)).Name}!");
                        return;
                    default:
                    {
                        string content = null;
                        if (clubs.Count != 0)
                            foreach (var x in clubs)
                                content += $"{x.ClubId} - {(await db.GetClubAsync(x.ClubId, Context.Guild)).Name}\n";
                        await Context.ReplyAsync(new EmbedBuilder().CreateDefault(content, Context.Guild.Id)
                            .WithTitle("Reply with the ID of club you wish to leave")
                            .WithFooter("Exit to cancel"));
                        var status = true;
                        var retries = 0;
                        while (status)
                            try
                            {
                                retries++;
                                var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(30));
                                if (response == null)
                                {
                                    if (retries >= 3) status = false;
                                    continue;
                                }

                                if (response.Content.ToLower() == "exit") return;
                                if (!int.TryParse(response.Content, out var result)) continue;
                                club = clubs.FirstOrDefault(x => x.ClubId == result);
                                if (club == null) continue;
                                await _management.RemoveUserAsync(db, context.User, context.Guild, club);
                                await Context.ReplyAsync(
                                    $"Successfully left {(await db.GetClubAsync(club.ClubId, Context.Guild)).Name}",
                                    Color.Green.RawValue);
                                status = false;
                            }
                            catch(Exception e)
                            {
                                _log.LogAction(LogLevel.Error, e.ToString(), "Club user leave");
                            }

                        break;
                    }
                }
            }
        }

        public async Task PromoteAsync(ICommandContext context, IGuildUser user)
        {
            if (context.User == user) return;
            using (var db = new DbService())
            {
                var club = await db.IsClubLeader(context.Guild.Id, context.User.Id);
                if (club == null)
                {
                    await Context.ReplyAsync("You can't use this command as you're not a leader of any clubs",
                        Color.Red.RawValue);
                    return;
                }
                var clubUser =
                    await db.ClubPlayers.FirstOrDefaultAsync(x =>
                        x.GuildId == user.GuildId && x.UserId == user.Id && x.Rank <= 2);
                if (clubUser != null)
                {
                    await context.ReplyAsync($"{user.Mention} is already an officer in a club");
                    return;
                }
                var toPromote = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                    x.ClubId == club.Id && x.GuildId == user.GuildId && x.UserId == user.Id);
                if (toPromote == null)
                {
                    await Context.ReplyAsync($"{user.Mention} is not part of {club.Name}",
                        Color.Red.RawValue);
                    return;
                }

                if (toPromote.Rank == 2)
                {
                    await context.ReplyAsync($"Are you sure you want to transfer ownership of {club.Name} to {user.Mention}? (y/n)");
                    var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(45));
                    if (response == null || response.Content.ToLower() != "y")
                    {
                        await _management.PromoteUserAsync(db, user, toPromote, club);
                        await context.ReplyAsync("Cancelling...");
                        return;
                    }

                    await context.ReplyAsync($"Transferred ownership of {club.Name} to {user.Mention}");
                }
                else
                {
                    await _management.PromoteUserAsync(db, user, toPromote, club);
                    await context.ReplyAsync($"Promoted {user.Mention} to rank 2.");
                }
                
            }
        }

        public async Task DemoteAsync(ICommandContext context, IGuildUser user)
        {
            if (context.User == user) return;
            using (var db = new DbService())
            {
                var club = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                if (club == null) return;
                var toDemote = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                    x.ClubId == club.Id && x.GuildId == user.GuildId && x.UserId == user.Id);
                if (toDemote == null)
                {
                    await Context.ReplyAsync($"Can't demote {user.Mention} because he/she is not part of {club.Name}",
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
                await Context.ReplyAsync($"Demoted {user.Mention} down to rank 3 in {club.Name}",
                    Color.Green.RawValue);
            }
        }

        public async Task BlackListUserAsync(ICommandContext context, IGuildUser blacklistUser, string reason)
        {
            if (context.User == blacklistUser) return;
            using (var db = new DbService())
            {
                var club = await db.IsClubLeader(context.Guild.Id, context.User.Id);
                if (club == null) return;
                var toBlacklist = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                    x.ClubId == club.Id && x.UserId == blacklistUser.Id && x.GuildId == context.Guild.Id);
                if (toBlacklist == null) return;
                var blacklist = await _management.BlackListUserAsync(db, blacklistUser, context.User as IGuildUser, club.Id, reason);
                if (!blacklist)
                {
                    await context.ReplyAsync(
                        "User is already blacklisted. Do you wish to remove it? (y/n)");
                    var response = await NextMessageAsync();
                    if (response == null || response.Content.IsNullOrWhiteSpace()) return;
                    if (response.Content.ToLower() != "y")
                    {
                        await context.ReplyAsync("User stays blacklisted");
                    }
                    await _management.RemoveBlackListUserAsync(db, blacklistUser, club.Id);
                    await context.ReplyAsync($"Removed blacklist for {blacklistUser.Mention} in {club.Name}", Color.Green.RawValue);
                }

                await context.ReplyAsync($"Blacklisted {blacklistUser.Mention} from {club.Name}", Color.Green.RawValue);
            }
        }

        public async Task GetBlackListAsync(ICommandContext context)
        {
            using (var db = new DbService())
            {
                var club = await db.IsClubLeader(context.Guild.Id, context.User.Id);
                if (club == null) return;
                var blacklist = await db.ClubBlacklists.Where(x => x.ClubId == club.Id).ToListAsync();
                if (blacklist == null || blacklist.Count == 0)
                {
                    await context.ReplyAsync("No users currently blacklisted");
                    return;
                }

                var result = new List<string>();
                foreach (var x in blacklist)
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine(
                        $"{x.BlackListUser} blacklisted by {x.IssuedUser} on {x.Time.Humanize()} ({x.Time})");
                    stringBuilder.AppendLine($"Reason: {x.Reason}");
                    stringBuilder.AppendLine();
                    result.Add(stringBuilder.ToString());
                }

                await PagedReplyAsync(result.PaginateBuilder(context.Guild.Id, context.Guild,
                    $"Blacklisted users for {club.Name}"));
            }
        }

        private async Task<int> GetUsersOfLevelAsync(DbService db, int level, IEnumerable<ClubUser> players)
        {
            var nr = 0;
            foreach (var x in players)
            {
                var user = await db.GetOrCreateUserData(x.GuildId, x.UserId);
                if (user.Level >= level) nr++;
            }

            return nr;
        }
    }
}