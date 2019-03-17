using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Modules.Club.Handler;
using Hanekawa.Preconditions;
using System.Threading.Tasks;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Club;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Services.Logging;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz.Util;

namespace Hanekawa.Modules.Club
{
    [RequireContext(ContextType.Guild)]
    [Name("Club")]
    public class Club : InteractiveBase
    {
        private readonly OverwritePermissions _allowOverwrite =
            new OverwritePermissions(addReactions: PermValue.Allow,
                attachFiles: PermValue.Allow, embedLinks: PermValue.Allow, viewChannel: PermValue.Allow);

        private readonly OverwritePermissions _denyOverwrite = new OverwritePermissions(
            addReactions: PermValue.Deny, attachFiles: PermValue.Deny,
            embedLinks: PermValue.Deny, viewChannel: PermValue.Deny);

        private readonly Admin _admin;
        private readonly Advertise _advertise;
        private readonly Management _management;
        private readonly LogService _log;

        public Club(Admin admin, Advertise advertise, Management management, LogService log)
        {
            _admin = admin;
            _advertise = advertise;
            _management = management;
            _log = log;
        }

        [Name("Create")]
        [Command("club create", RunMode = RunMode.Async)]
        [Summary("Creates a club")]
        [Remarks("h.club create Fan service club")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task CreateClub([Remainder] string name) => await _admin.CreateAsync(Context, name);

        [Name("Club add")]
        [Command("club add", RunMode = RunMode.Async)]
        [Summary("Adds a member to your club")]
        [Remarks("h.club add @bob#0000")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task AddClubMemberAsync(IGuildUser user)
        {
            if (user == Context.User) return;
            using (var db = new DbService())
            {
                var clubUser =
                    await db.ClubPlayers.FirstOrDefaultAsync(x =>
                        x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id && x.Rank <= 2);
                if (clubUser == null) return;
                if (clubUser.Rank > 2)
                {
                    await Context.ReplyAsync("You're not high enough rank to use that command!", Color.Red.RawValue);
                    return;
                }
                var check = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                    x.UserId == user.Id && x.GuildId == user.GuildId && x.ClubId == clubUser.Id);
                if (check != null)
                {
                    await Context.ReplyAsync($"{user.Mention} is already a member of your club.");
                    return;
                }
                var clubData = await db.GetClubAsync(clubUser.ClubId, Context.Guild);
                await ReplyAsync(
                    $"{user.Mention}, {Context.User.Mention} has invited you to {clubData.Name}, do you accept? (y/n)");
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
                        await Context.ReplyAsync("Invite expired.", Color.Red.RawValue);
                        return;
                    }
                await Context.Channel.TriggerTypingAsync();
                await _management.AddUserAsync(db, user, clubUser);
                await Context.ReplyAsync($"Added {user.Mention} to {clubData.Name}", Color.Green.RawValue);
            }
        }

        [Name("Club remove")]
        [Command("club remove", RunMode = RunMode.Async)]
        [Alias("club kick")]
        [Summary("Removes a user from your club")]
        [Remarks("h.club remove @bob#0000")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task RemoveClubMemberAsync(IGuildUser user)
        {
            if (user == Context.User) return;
            using (var db = new DbService())
            {
                var club = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                if (club == null) return;
                await Context.Channel.TriggerTypingAsync();
                var response = await _management.RemoveUserAsync(db, user, club.Id);
                if (response == null)
                {
                    await Context.ReplyAsync($"Couldn't remove {user.Mention}", Color.Red.RawValue);
                    return;
                }

                await Context.ReplyAsync($"Removed {user.Mention} from {club.Name}", Color.Green.RawValue);
            }
        }

        [Name("Club leave")]
        [Command("club leave", RunMode = RunMode.Async)]
        [Summary("Leaves a club you're part of")]
        [Remarks("h.club leave")]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task LeaveClubAsync()
        {
            using (var db = new DbService())
            {
                var clubs = await db.ClubPlayers
                    .Where(x => x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id).ToListAsync();
                ClubUser club = null;
                switch (clubs.Count)
                {
                    case 0:
                        await Context.ReplyAsync("You're currently not in a club.", Color.Red.RawValue);
                        break;
                    case 1:
                        club = clubs.FirstOrDefault();
                        var clubInfo = await _management.RemoveUserAsync(db, Context.User, Context.Guild, club);
                        if (clubInfo != null)
                        {
                            await Context.ReplyAsync(
                                $"Successfully left {clubInfo.Name}!");
                        }
                        break;
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
                                await _management.RemoveUserAsync(db, Context.User, Context.Guild, club);
                                await Context.ReplyAsync(
                                    $"Successfully left {(await db.GetClubAsync(club.ClubId, Context.Guild)).Name}",
                                    Color.Green.RawValue);
                                status = false;
                            }
                            catch (Exception e)
                            {
                                _log.LogAction(LogLevel.Error, e.ToString(), "Club user leave");
                            }

                        break;
                    }
                }
            }
        }

        [Name("Club promote")]
        [Command("club promote", RunMode = RunMode.Async)]
        [Summary("Promotes someone to a higher rank")]
        [Remarks("h.club promote @bob#0000")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubPromoteAsync(IGuildUser user)
        {
            if (Context.User == user) return;
            using (var db = new DbService())
            {
                var club = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                if (club == null)
                {
                    await Context.ReplyAsync("You can't use this command as you're not a leader of any clubs",
                        Color.Red.RawValue);
                    return;
                }
                var clubUser =
                    await db.ClubPlayers.FirstOrDefaultAsync(x =>
                        x.GuildId == user.GuildId && x.UserId == user.Id && x.Rank <= 2);
                if (clubUser != null && clubUser.ClubId != club.Id)
                {
                    await Context.ReplyAsync($"{user.Mention} is already an officer in a club");
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
                    await Context.ReplyAsync($"Are you sure you want to transfer ownership of {club.Name} to {user.Mention}? (y/n)");
                    var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(45));
                    if (response == null || response.Content.ToLower() != "y")
                    {
                        await Context.ReplyAsync("Cancelling...");
                        return;
                    }
                    await _management.PromoteUserAsync(db, user, toPromote, club);
                    await Context.ReplyAsync($"Transferred ownership of {club.Name} to {user.Mention}");
                }
                else
                {
                    await _management.PromoteUserAsync(db, user, toPromote, club);
                    await Context.ReplyAsync($"Promoted {user.Mention} to rank 2.");
                }

            }
        }

        [Name("Club demote")]
        [Command("club demote", RunMode = RunMode.Async)]
        [Summary("Demotes someone to a lower rank")]
        [Remarks("club demote @bob#0000")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubDemoteAsync(IGuildUser user) => await _admin.DemoteAsync(Context, user);

        [Name("Club channel")]
        [Command("club channel", RunMode = RunMode.Async)]
        [Summary("Creates a channel and role for the club")]
        [Remarks("h.club channel")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubChannelAsync()
        {
            using (var db = new DbService())
            {
                var leader = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                if (leader.Channel.HasValue) return;
                var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
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

                var channel = await Context.Guild.CreateTextChannelAsync(leader.Name,
                    x => x.CategoryId = cfg.ChannelCategory.Value);
                await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, _denyOverwrite);
                leader.Channel = channel.Id;
                if (cfg.RoleEnabled)
                {
                    var role = await Context.Guild.CreateRoleAsync(leader.Name, GuildPermissions.None);
                    await channel.AddPermissionOverwriteAsync(role, _allowOverwrite);
                    leader.Role = role.Id;
                    foreach (var x in users)
                        try
                        {
                            await Context.Guild.GetUser(x.UserId).TryAddRoleAsync(role)
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
                            await channel.AddPermissionOverwriteAsync(Context.Guild.GetUser(x.UserId),
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

        [Name("Club list")]
        [Command("club list", RunMode = RunMode.Async)]
        [Alias("club clubs")]
        [Summary("Paginates all clubs")]
        [Remarks("h.club list")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubListAsync()
        {
            using (var db = new DbService())
            {
                var clubs = await db.ClubInfos.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                if (clubs.Count == 0)
                {
                    await Context.ReplyAsync("No clubs on this server");
                    return;
                }

                var pages = new List<string>();
                foreach (var x in clubs)
                {
                    var leader = (Context.Guild.GetUser(x.LeaderId)).Mention ??
                                 "Couldn't find user or left server.";
                    pages.Add($"**{x.Name} (id: {x.Id})**\n" +
                              $"Members: {await db.ClubPlayers.CountAsync(y => y.GuildId == Context.Guild.Id && y.ClubId == x.Id)}\n" +
                              $"Leader {leader}\n");
                }

                await PagedReplyAsync(pages.PaginateBuilder(Context.Guild.Id, Context.Guild,
                    $"Clubs in {Context.Guild.Name}"));
            }
        }

        [Name("Club check")]
        [Command("club check", RunMode = RunMode.Async)]
        [Summary("Checks specific club information")]
        [Remarks("h.club check 15")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubCheckAsync(int id)
        {
            using (var db = new DbService())
            {
                var club = await db.GetClubAsync(id, Context.Guild);
                if (club == null)
                {
                    await Context.ReplyAsync("Couldn't find a club with that ID.", Color.Red.RawValue);
                    return;
                }

                var clubUsers =
                    await db.ClubPlayers.Where(x => x.GuildId == Context.Guild.Id && x.ClubId == club.Id).ToListAsync();
                string officers = null;
                foreach (var x in clubUsers.Where(x => x.Rank == 2))
                {
                    officers += $"{Context.Guild.GetUser(x.UserId).Mention}\n";
                }

                if (officers == null) officers += "No officers";

                var embed = new EmbedBuilder
                {
                    ThumbnailUrl = club.ImageUrl,
                    Timestamp = club.CreationDate,
                    Author = new EmbedAuthorBuilder { IconUrl = club.IconUrl, Name = $"{club.Name} (ID:{club.Id})"},
                    Footer = new EmbedFooterBuilder { Text = "Created:"},
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder { IsInline = false, Name = "Leader", Value = $"{Context.Guild.GetUser(club.LeaderId).Mention ?? "Couldn't find user or left server."}"},
                        new EmbedFieldBuilder { IsInline = false, Name = "Officers", Value = officers}
                    }
                }.CreateDefault(club.Description, Context.Guild.Id);
                await Context.ReplyAsync(embed);
            }
        }

        [Name("Club name")]
        [Command("club name", RunMode = RunMode.Async)]
        [Alias("club name")]
        [Summary("Changes club name")]
        [Remarks("h.club name Google Town")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubNameChangeAsync([Remainder] string content) =>
            await _advertise.NameAsync(Context, content);

        [Name("Club description")]
        [Command("club description", RunMode = RunMode.Async)]
        [Alias("club desc")]
        [Summary("Sets description of a club")]
        [Remarks("h.club desc this is a description")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubDescriptionAsync([Remainder] string content) =>
            await _advertise.DescriptionAsync(Context, content);

        [Name("Club image")]
        [Command("club image", RunMode = RunMode.Async)]
        [Alias("club pic", "cimage")]
        [Summary("Sets a picture to a club")]
        [Remarks("h.club pic https://i.imgur.com/p3Xxvij.png")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubImageAsync(string image) => await _advertise.ImageAsync(Context, image);

        [Name("Club icon")]
        [Command("club icon", RunMode = RunMode.Async)]
        [Alias("cicon")]
        [Summary("Sets a icon to a club")]
        [Remarks("h.club icon https://i.imgur.com/p3Xxvij.png")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubIconAsync(string image) => await _advertise.IconAsync(Context, image);

        [Name("Club public")]
        [Command("club public", RunMode = RunMode.Async)]
        [Summary("Toggles a club to be public or not")]
        [Remarks("h.club public")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubPublicAsync() => await _advertise.PublicAsync(Context);

        [Name("Club advertise")]
        [Command("club advertise", RunMode = RunMode.Async)]
        [Alias("club post")]
        [Summary("Posts a advertisement of club to designated advertisement channel")]
        [Remarks("h.club advertise")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubAdvertiseAsync() => await _advertise.AdvertiseAsync(Context);

        [Name("Club blacklist")]
        [Command("club blacklist", RunMode = RunMode.Async)]
        [Alias("cb")]
        [Summary("Blacklist a user from their club")]
        [Remarks("h.cb @bob#0000")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task BlackListUser(IGuildUser user, [Remainder] string reason = null)
        {
            if (Context.User == user) return;
            using (var db = new DbService())
            {
                var club = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                if (club == null) return;
                var toBlacklist = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                    x.ClubId == club.Id && x.UserId == user.Id && x.GuildId == Context.Guild.Id);
                if (toBlacklist == null) return;
                var blacklist = await _management.BlackListUserAsync(db, user, Context.User as IGuildUser, club.Id, reason);
                if (!blacklist)
                {
                    await Context.ReplyAsync(
                        "User is already blacklisted. Do you wish to remove it? (y/n)");
                    var response = await NextMessageAsync();
                    if (response == null || response.Content.IsNullOrWhiteSpace()) return;
                    if (response.Content.ToLower() != "y")
                    {
                        await Context.ReplyAsync("User stays blacklisted");
                    }
                    await _management.RemoveBlackListUserAsync(db, user, club.Id);
                    await Context.ReplyAsync($"Removed blacklist for {user.Mention} in {club.Name}", Color.Green.RawValue);
                }

                await Context.ReplyAsync($"Blacklisted {user.Mention} from {club.Name}", Color.Green.RawValue);
            }
        }

        [Name("Club blacklist")]
        [Command("club blacklist", RunMode = RunMode.Async)]
        [Alias("cb")]
        [Summary("Gets current blacklist for their club")]
        [Remarks("h.cb")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task GetBlackList()
        {
            using (var db = new DbService())
            {
                var club = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                if (club == null) return;
                var blacklist = await db.ClubBlacklists.Where(x => x.ClubId == club.Id).ToListAsync();
                if (blacklist == null || blacklist.Count == 0)
                {
                    await Context.ReplyAsync("No users currently blacklisted");
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

                await PagedReplyAsync(result.PaginateBuilder(Context.Guild.Id, Context.Guild,
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