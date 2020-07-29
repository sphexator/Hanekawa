﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Club;
using Hanekawa.Shared.Command.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using Quartz.Util;

namespace Hanekawa.Bot.Modules.Club
{
    public partial class Club
    {
        [Name("Create")]
        [Command("clubcreate", "ccreate")]
        [Description("Creates a club")]
        [RequiredChannel]
        public async Task CreateClub([Remainder] string name)
        {
            if (name.IsNullOrWhiteSpace()) return;
            
            await using var db = Context.ServiceScope.ServiceProvider.GetRequiredService<DbService>();
            var userData = await db.GetOrCreateUserData(Context.Member);
            var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
            if (userData.Level < cfg.ChannelRequiredLevel)
            {
                await Context.ReplyAsync(
                    $"You do not meet the requirement to make a club (Level {cfg.ChannelRequiredLevel}).",
                    Color.Red);
                return;
            }

            var leaderCheck = await db.ClubInfos.FirstOrDefaultAsync(x =>
                x.GuildId == Context.Guild.Id.RawValue && x.LeaderId == Context.User.Id.RawValue);
            if (leaderCheck != null)
            {
                await Context.ReplyAsync("You're already a leader of a club, you can't create multiple clubs.",
                    Color.Red);
                return;
            }

            var club = await db.CreateClub(Context.User, Context.Guild, name, DateTimeOffset.UtcNow);
            var data = new ClubUser
            {
                ClubId = club.Id,
                GuildId = Context.Guild.Id.RawValue,
                JoinDate = DateTimeOffset.UtcNow,
                Rank = 1,
                UserId = Context.User.Id.RawValue
            };
            await db.ClubPlayers.AddAsync(data);
            await db.SaveChangesAsync();
            await Context.ReplyAsync($"Successfully created club {name} !", Color.Green);
        }

        [Name("Club Add")]
        [Command("clubadd", "cadd")]
        [Description("Adds a member to your club")]
        [RequiredChannel]
        public async Task AddClubMemberAsync(CachedMember user)
        {
            if (user == Context.User) return;
            
            await using var db = Context.ServiceScope.ServiceProvider.GetRequiredService<DbService>();
            var clubUser =
                await db.ClubPlayers.FirstOrDefaultAsync(x =>
                    x.GuildId == Context.Guild.Id.RawValue && x.UserId == Context.User.Id.RawValue && x.Rank <= 2);
            if (clubUser == null) return;
            if (clubUser.Rank > 2)
            {
                await Context.ReplyAsync("You're not high enough rank to use that command!", Color.Red);
                return;
            }

            var check = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                x.UserId == user.Id.RawValue && x.GuildId == user.Guild.Id.RawValue && x.ClubId == clubUser.Id);
            if (check != null)
            {
                await Context.ReplyAsync($"{user.Mention} is already a member of your club.");
                return;
            }

            var clubData = await db.GetClubAsync(Context.Member, clubUser.ClubId);
            await Context.ReplyAsync(
                $"{user.Mention}, {Context.User.Mention} has invited you to {clubData.Name}, do you accept? (y/n)");
            var status = true;
            while (status)
                try
                {
                    var response = await Context.Bot.GetInteractivity().WaitForMessageAsync(x =>
                        x.Message.Author.Id.RawValue == user.Id.RawValue && x.Message.Guild.Id.RawValue == user.Guild.Id.RawValue, TimeSpan.FromMinutes(1));

                    if (response.Message.Content.ToLower() == "y") status = false;
                    if (response.Message.Content.ToLower() == "n") return;
                }
                catch
                {
                    await Context.ReplyAsync("Invite expired.", Color.Red);
                    return;
                }

            await Context.Channel.TriggerTypingAsync();
            await _club.AddUserAsync(user, clubData.Id, db);
            await Context.ReplyAsync($"Added {user.Mention} to {clubData.Name}", Color.Green);
        }

        [Name("Club Remove")]
        [Command("clubremove", "clubkick", "ckick")]
        [Description("Removes a user from your club")]
        [RequiredChannel]
        public async Task RemoveClubMemberAsync(CachedMember user)
        {
            if (user == Context.Member) return;
            
            await using var db = Context.ServiceScope.ServiceProvider.GetRequiredService<DbService>();
            var club = await db.ClubInfos.FirstOrDefaultAsync(x =>
                x.GuildId == Context.Guild.Id.RawValue && x.LeaderId == Context.User.Id.RawValue);
            if (club == null) return;
            await Context.Channel.TriggerTypingAsync();
            if (!await _club.RemoveUserAsync(user, user.Guild, club.Id, db))
            {
                await Context.ReplyAsync($"Couldn't remove {user.Mention}", Color.Red);
                return;
            }

            await Context.ReplyAsync($"Removed {user.Mention} from {club.Name}", Color.Green);
        }

        [Name("Club Leave")]
        [Command("clubleave")]
        [Description("Leaves a club you're part of")]
        [RequiredChannel]
        public async Task LeaveClubAsync(int id)
        {
            
            await using var db = Context.ServiceScope.ServiceProvider.GetRequiredService<DbService>();
            var clubUser = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                x.GuildId == Context.Guild.Id.RawValue && x.UserId == Context.User.Id.RawValue && x.ClubId == id);
            var club = await db.ClubInfos.FirstOrDefaultAsync(x =>
                x.GuildId == Context.Guild.Id.RawValue && x.Id == clubUser.ClubId);
            await _club.RemoveUserAsync(Context.Member, Context.Guild, club.Id, db);
            await Context.ReplyAsync($"Successfully left {club.Name}", Color.Green);
        }

        [Name("Club Leave")]
        [Command("clubleave")]
        [Description("Leaves a club you're part of")]
        [RequiredChannel]
        public async Task LeaveClubAsync()
        {
            
            await using var db = Context.ServiceScope.ServiceProvider.GetRequiredService<DbService>();
            var clubs = await db.ClubPlayers.Where(
                x => x.GuildId == Context.Guild.Id.RawValue && x.UserId == Context.User.Id.RawValue).ToListAsync();
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
                    x.GuildId == Context.Guild.Id.RawValue && x.Id == clubs[i].ClubId);
                clubInfos.Add(clubInfo);
                str.AppendLine($"ID: {clubInfo.Id}: {clubInfo.Name}");
            }

            await Context.ReplyAsync($"Which club would you like to leave? Provide ID\n{str}");
            var response = await Context.Bot.GetInteractivity().WaitForMessageAsync(x =>
                x.Message.Author.Id.RawValue == Context.Member.Id.RawValue && x.Message.Guild.Id.RawValue == Context.Member.Guild.Id.RawValue, TimeSpan.FromMinutes(1));
            if (response == null || response.Message.Content.IsNullOrWhiteSpace())
            {
                await Context.ReplyAsync("Timed out", Color.Red);
                return;
            }

            var check = int.TryParse(response.Message.Content, out var result);
            if (!check) return;
            var club = clubInfos.FirstOrDefault(x => x.Id == result);
            if (club == null)
            {
                await Context.ReplyAsync("Couldn't find a club with that ID", Color.Red);
                return;
            }

            var leave = await _club.RemoveUserAsync(Context.User, Context.Guild, club.Id, db);
            if (leave) await Context.ReplyAsync($"Successfully left {club.Name}", Color.Green);
            else await Context.ReplyAsync($"Something went wrong, couldn't leave {club.Name}", Color.Red);
        }

        [Name("Club Promote")]
        [Command("clubpromote", "cprom")]
        [Description("Promotes someone to a higher rank")]
        [RequiredChannel]
        public async Task ClubPromoteAsync(CachedMember user)
        {
            if (Context.User == user) return;
            
            await using var db = Context.ServiceScope.ServiceProvider.GetRequiredService<DbService>();
            var club = await db.ClubInfos.FirstOrDefaultAsync(x =>
                x.GuildId == Context.Guild.Id.RawValue && x.LeaderId == Context.Guild.Id.RawValue);
            if (club == null)
            {
                await Context.ReplyAsync("You can't use this command as you're not a leader of any clubs",
                    Color.Red);
                return;
            }

            var clubUser =
                await db.ClubPlayers.FirstOrDefaultAsync(x =>
                    x.GuildId == user.Guild.Id.RawValue && x.UserId == user.Id.RawValue && x.Rank <= 2);
            if (clubUser != null && clubUser.ClubId != club.Id)
            {
                await Context.ReplyAsync($"{user.Mention} is already an officer in a club");
                return;
            }

            var toPromote = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                x.ClubId == club.Id && x.GuildId == user.Guild.Id.RawValue && x.UserId == user.Id.RawValue);
            if (toPromote == null)
            {
                await Context.ReplyAsync($"{user.Mention} is not part of {club.Name}",
                    Color.Red);
                return;
            }

            if (toPromote.Rank == 2)
            {
                await Context.ReplyAsync(
                    $"Are you sure you want to transfer ownership of {club.Name} to {user.Mention}? (y/n)");
                var response = await Context.Bot.GetInteractivity()
                    .WaitForMessageAsync(x => x.Message.Guild.Id.RawValue == user.Guild.Id.RawValue, TimeSpan.FromMinutes(1));
                if (response == null || response.Message.Content.ToLower() != "y")
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

        [Name("Club Demote")]
        [Command("clubdemote", "cdem")]
        [Description("Demotes someone to a lower rank")]
        [RequiredChannel]
        public async Task ClubDemoteAsync(CachedMember user)
        {
            if (Context.User == user) return;
            
            await using var db = Context.ServiceScope.ServiceProvider.GetRequiredService<DbService>();
            var club = await db.ClubInfos.FirstOrDefaultAsync(x =>
                x.GuildId == Context.Guild.Id.RawValue && x.LeaderId == Context.User.Id.RawValue);
            if (club == null) return;
            var toDemote = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                x.ClubId == club.Id && x.GuildId == user.Guild.Id.RawValue && x.UserId == user.Id.RawValue);
            if (toDemote == null)
            {
                await Context.ReplyAsync($"Can't demote {user.Mention} because he/she is not part of {club.Name}",
                    Color.Red);
                return;
            }

            if (toDemote.Rank == 3)
            {
                await Context.ReplyAsync($"Cannot demote {user.Mention} any further. (Already lowest rank)");
                return;
            }

            if (toDemote.Rank == 1) return;
            await _club.DemoteAsync(toDemote, db);
            await Context.ReplyAsync($"Demoted {user.Mention} down to rank 3 in {club.Name}",
                Color.Green);
        }

        [Name("Club Blacklist")]
        [Command("clubblacklist", "cblacklist", "cb")]
        [Description("Blacklist a user from their club")]
        [RequiredChannel]
        public async Task BlackListUser(CachedMember user, [Remainder] string reason = null)
        {
            if (Context.User == user) return;
            
            await using var db = Context.ServiceScope.ServiceProvider.GetRequiredService<DbService>();
            var club = await db.ClubInfos.FirstOrDefaultAsync(x =>
                x.GuildId == Context.Guild.Id.RawValue && x.LeaderId == Context.User.Id.RawValue);
            if (club == null) return;
            var toBlacklist = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                x.ClubId == club.Id && x.UserId == user.Id.RawValue && x.GuildId == Context.Guild.Id.RawValue);
            if (toBlacklist == null) return;
            var blacklist = await _club.AddBlacklist(user, Context.Member, Context.Guild, club, db, reason);
            if (!blacklist)
            {
                await Context.ReplyAsync(
                    "User is already blacklisted. Do you wish to remove it? (y/n)");
                var response = await Context.Bot.GetInteractivity().WaitForMessageAsync(x =>
                    x.Message.Author.Id.RawValue == Context.Member.Id.RawValue && x.Message.Guild.Id.RawValue == user.Guild.Id.RawValue, TimeSpan.FromMinutes(1));
                if (response == null || response.Message.Content.IsNullOrWhiteSpace()) return;
                if (response.Message.Content.ToLower() != "y")
                {
                    await Context.ReplyAsync("User stays blacklisted");
                    return;
                }
                await _club.RemoveBlacklist(user, Context.Guild, club, db);
                await Context.ReplyAsync($"Removed blacklist for {user.Mention} in {club.Name}",
                    Color.Green);
            }

            await Context.ReplyAsync($"Blacklisted {user.Mention} from {club.Name}", Color.Green);
        }

        [Name("Club Blacklist")]
        [Command("clubblacklist", "cblacklist", "cb")]
        [Description("Gets current blacklist for their club")]
        [RequiredChannel]
        public async Task GetBlackList()
        {
            
            await using var db = Context.ServiceScope.ServiceProvider.GetRequiredService<DbService>();
            var club = await db.ClubInfos.FirstOrDefaultAsync(x =>
                x.GuildId == Context.Guild.Id.RawValue && x.LeaderId == Context.User.Id.RawValue);
            if (club == null) return;
            var blacklist = await db.ClubBlacklists.Where(x => x.ClubId == club.Id).ToListAsync();
            if (blacklist == null || blacklist.Count == 0)
            {
                await Context.ReplyAsync("No users currently blacklisted");
                return;
            }

            var result = new List<string>();
            for (var i = 0; i < blacklist.Count; i++)
            {
                var x = blacklist[i];
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(
                    $"{x.BlackListUser} blacklisted by {x.IssuedUser} on {x.Time.Humanize()} ({x.Time})");
                stringBuilder.AppendLine($"Reason: {x.Reason}");
                result.Add(stringBuilder.ToString());
            }

            await Context.PaginatedReply(result, Context.Guild, $"Blacklisted users for {club.Name}");
        }
    }
}