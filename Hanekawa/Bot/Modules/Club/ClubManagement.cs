using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Core.Interactive.Criteria;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Club;
using Hanekawa.Extensions.Embed;
using Microsoft.EntityFrameworkCore;
using Qmmands;
using Quartz.Util;

namespace Hanekawa.Bot.Modules.Club
{
    public partial class Club
    {
        [Name("Create")]
        [Command("club create")]
        [Description("Creates a club")]
        [Remarks("club create Fan service club")]
        [RequiredChannel]
        public async Task CreateClub([Remainder] string name)
        {
            if (name.IsNullOrWhiteSpace()) return;
            using (var db = new DbService())
            {
                var userData = await db.GetOrCreateUserData(Context.User);
                var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
                if (userData.Level < cfg.ChannelRequiredLevel)
                {
                    await Context.ReplyAsync($"You do not meet the requirement to make a club (Level {cfg.ChannelRequiredLevel}).",
                        Color.Red.RawValue);
                    return;
                }

                var leaderCheck = await db.ClubInfos.FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id && x.LeaderId == Context.User.Id);
                if (leaderCheck != null)
                {
                    await Context.ReplyAsync("You're already a leader of a club, you can't create multiple clubs.",
                        Color.Red.RawValue);
                    return;
                }

                var club = await db.CreateClub(Context.User, Context.Guild, name, DateTimeOffset.UtcNow);
                var data = new ClubUser
                {
                    ClubId = club.Id,
                    GuildId = Context.Guild.Id,
                    JoinDate = DateTimeOffset.UtcNow,
                    Rank = 1,
                    UserId = Context.User.Id,
                    Id = await db.ClubPlayers.CountAsync() + 1
                };
                await db.ClubPlayers.AddAsync(data);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Successfully created club {name} !", Color.Green.RawValue);
            }
        }

        [Name("Club add")]
        [Command("club add")]
        [Description("Adds a member to your club")]
        [Remarks("club add @bob#0000")]
        [RequiredChannel]
        public async Task AddClubMemberAsync(SocketGuildUser user)
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
                    x.UserId == user.Id && x.GuildId == user.Guild.Id && x.ClubId == clubUser.Id);
                if (check != null)
                {
                    await Context.ReplyAsync($"{user.Mention} is already a member of your club.");
                    return;
                }
                var clubData = await db.GetClubAsync(Context.User, clubUser.ClubId);
                await Context.ReplyAsync(
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
                await _club.AddUserAsync(user, clubData.Id, db);
                await Context.ReplyAsync($"Added {user.Mention} to {clubData.Name}", Color.Green.RawValue);
            }
        }

        [Name("Club remove")]
        [Command("club remove", "club kick")]
        [Description("Removes a user from your club")]
        [Remarks("club remove @bob#0000")]
        [RequiredChannel]
        public async Task RemoveClubMemberAsync(SocketGuildUser user)
        {
            if (user == Context.User) return;
            using (var db = new DbService())
            {
                var club = await db.ClubInfos.FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id && x.LeaderId == Context.User.Id);
                if (club == null) return;
                await Context.Channel.TriggerTypingAsync();
                if (!await _club.RemoveUserAsync(user, club.Id, db))
                {
                    await Context.ReplyAsync($"Couldn't remove {user.Mention}", Color.Red.RawValue);
                    return;
                }

                await Context.ReplyAsync($"Removed {user.Mention} from {club.Name}", Color.Green.RawValue);
            }
        }

        [Name("Club leave")]
        [Command("club leave")]
        [Description("Leaves a club you're part of")]
        [Remarks("club leave")]
        [RequiredChannel]
        public async Task LeaveClubAsync(int id)
        {
            using (var db = new DbService())
            {
                var clubUser = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                    x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id && x.ClubId == id);
                var club = await db.ClubInfos.FirstOrDefaultAsync(x =>
                    x.GuildId == Context.Guild.Id && x.Id == clubUser.ClubId);
                await _club.RemoveUserAsync(Context.User, club.Id, db);
                await Context.ReplyAsync($"Successfully left {club.Name}", Color.Green.RawValue);
            }
        }

        [Name("Club leave")]
        [Command("club leave")]
        [Description("Leaves a club you're part of")]
        [Remarks("club leave")]
        [RequiredChannel]
        public async Task LeaveClubAsync()
        {
            using (var db = new DbService())
            {
                var clubs = await db.ClubPlayers.Where(
                    x => x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id).ToListAsync();
                if (clubs.Count == 0)
                {
                    await Context.ReplyAsync("You're currently not in any clubs within this guild");
                    return;
                }
                var clubInfos = new List<ClubInformation>();
                var str = new StringBuilder();
                for (var i = 0; i < clubs.Count; i++)
                {
                    var clubInfo = await db.ClubInfos.FirstOrDefaultAsync(x =>
                        x.GuildId == Context.Guild.Id && x.Id == clubs[i].ClubId);
                    clubInfos.Add(clubInfo);
                    str.AppendLine($"ID: {clubInfo.Id}: {clubInfo.Name}");
                }

                await Context.ReplyAsync($"Which club would you like to leave? Provide ID\n{str}");
                var response = await NextMessageAsync();
                if (response == null || response.Content.IsNullOrWhiteSpace())
                {
                    await Context.ReplyAsync("Timed out", Color.Red.RawValue);
                    return;
                }

                var check = int.TryParse(response.Content, out var result);
                var club = clubInfos.FirstOrDefault(x => x.Id == result);
                if(!check || club == null)
                {
                    await Context.ReplyAsync("Couldn't find a club with that Id", Color.Red.RawValue);
                    return;
                }

            }
        }

        [Name("Club promote")]
        [Command("club promote")]
        [Description("Promotes someone to a higher rank")]
        [Remarks("h.club promote @bob#0000")]
        [RequiredChannel]
        public async Task ClubPromoteAsync(SocketGuildUser user)
        {
            if (Context.User == user) return;
            using (var db = new DbService())
            {
                var club = await db.ClubInfos.FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id && x.LeaderId == Context.Guild.Id);
                if (club == null)
                {
                    await Context.ReplyAsync("You can't use this command as you're not a leader of any clubs",
                        Color.Red.RawValue);
                    return;
                }
                var clubUser =
                    await db.ClubPlayers.FirstOrDefaultAsync(x =>
                        x.GuildId == user.Guild.Id && x.UserId == user.Id && x.Rank <= 2);
                if (clubUser != null && clubUser.ClubId != club.Id)
                {
                    await Context.ReplyAsync($"{user.Mention} is already an officer in a club");
                    return;
                }
                var toPromote = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                    x.ClubId == club.Id && x.GuildId == user.Guild.Id && x.UserId == user.Id);
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
                    await _club.PromoteUserAsync(user, toPromote, club, db);
                    await Context.ReplyAsync($"Transferred ownership of {club.Name} to {user.Mention}");
                }
                else
                {
                    await _club.PromoteUserAsync(user, toPromote, club, db);
                    await Context.ReplyAsync($"Promoted {user.Mention} to rank 2.");
                }

            }
        }

        [Name("Club demote")]
        [Command("club demote")]
        [Description("Demotes someone to a lower rank")]
        [Remarks("club demote @bob#0000")]
        [RequiredChannel]
        public async Task ClubDemoteAsync(SocketGuildUser user)
        {
            if (Context.User == user) return;
            using (var db = new DbService())
            {
                var club = await db.ClubInfos.FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id && x.LeaderId == Context.User.Id);
                if (club == null) return;
                var toDemote = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                    x.ClubId == club.Id && x.GuildId == user.Guild.Id && x.UserId == user.Id);
                if (toDemote == null)
                {
                    await Context.ReplyAsync($"Can't demote {user.Mention} because he/she is not part of {club.Name}",
                        Color.Red.RawValue);
                    return;
                }

                if (toDemote.Rank == 3)
                {
                    await Context.ReplyAsync($"Cannot demote {user.Mention} any further. (Already lowest rank)");
                    return;
                }

                if (toDemote.Rank == 1) return;
                await _club.DemoteAsync(user, toDemote, club, db);
                await Context.ReplyAsync($"Demoted {user.Mention} down to rank 3 in {club.Name}",
                    Color.Green.RawValue);
            }
        }
    }
}
