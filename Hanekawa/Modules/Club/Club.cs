using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Club;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Preconditions;
using Hanekawa.Services.Club;
using Microsoft.EntityFrameworkCore;
using static Hanekawa.Services.Club.ClubService;

namespace Hanekawa.Modules.Club
{
    [Group("club")]
    [RequireContext(ContextType.Guild)]
    public class Club : InteractiveBase
    {
        private readonly ClubService _clubService;
        private readonly DbService _db;
        public Club(ClubService clubService)
        {
            _clubService = clubService;
        }

        [Command("create", RunMode = RunMode.Async)]
        [Summary("Creates a club")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task CreateClub([Remainder] string name)
        {
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                if (userdata.Level < 40)
                {
                    await Context.ReplyAsync("You do not meet the requirement to make a club (Level 40).",
                        Color.Red.RawValue);
                    return;
                }

                var leaderCheck = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                if (leaderCheck != null)
                {
                    await Context.ReplyAsync("You're already a leader of a club, you can't create multiple clubs.",
                        Color.Red.RawValue);
                    return;
                }

                var club = await db.CreateClub(Context.User, Context.Guild, name, DateTimeOffset.UtcNow);
                var data = new ClubPlayer
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

        [Command("add", RunMode = RunMode.Async)]
        [Summary("Adds a member to your club")]
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

                var data = new ClubPlayer
                {
                    ClubId = clubUser.ClubId,
                    GuildId = Context.Guild.Id,
                    JoinDate = DateTimeOffset.UtcNow,
                    Rank = 3,
                    UserId = user.Id,
                    Id = DateTime.UnixEpoch.Millisecond
                };
                await db.ClubPlayers.AddAsync(data);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Added {user.Mention} to {clubData.Name}", Color.Green.RawValue);
                if (clubData.RoleId.HasValue)
                {
                    var role = Context.Guild.GetRole(clubData.RoleId.Value);
                    if (role == null)
                    {
                        clubData.RoleId = null;
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        await user.AddRoleAsync(role);
                    }
                }
            }
        }

        [Command("remove", RunMode = RunMode.Async)]
        [Alias("kick")]
        [Summary("Removes a user from your club")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task RemoveClubMemberAsync(IGuildUser user)
        {
            if (user == Context.User) return;
            using (var db = new DbService())
            {
                var leader = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                if (leader == null) return;
                var clubUser = await db.ClubPlayers.FindAsync(Context.User.Id, Context.Guild.Id, leader.Id);
                if (clubUser == null)
                {
                    await Context.ReplyAsync($"Can't remove {user.Mention} because he/she is not part of {leader.Name}",
                        Color.Red.RawValue);
                    return;
                }

                db.ClubPlayers.Remove(clubUser);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Removed {user.Mention} from {leader.Name}", Color.Green.RawValue);
            }
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Summary("Leaves a club you're part of")]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task LeaveClubAsync()
        {
            using (var db = new DbService())
            {
                var clubs = await db.ClubPlayers
                    .Where(x => x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id).ToListAsync();
                var nr = 1;
                string content = null;
                if (clubs.Count != 0)
                    foreach (var x in clubs)
                    {
                        content += $"{x.ClubId} - {(await db.GetClubAsync(x.ClubId, Context.Guild)).Name}\n";
                        nr++;
                    }
                else content += "Currently not in any clubs";

                await Context.ReplyAsync(new EmbedBuilder().CreateDefault(content)
                    .WithTitle("Reply with the ID of club you wish to leave")
                    .WithFooter("Exit to cancel"));
                var status = true;
                try
                {
                    while (status)
                    {
                        var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(30));
                        if (response.Content.ToLower() == "exit") return;
                        if (!int.TryParse(response.Content, out var result)) continue;
                        var club = clubs.FirstOrDefault(x => x.ClubId == result);
                        if (club == null) continue;
                        db.ClubPlayers.Remove(club);
                        await db.SaveChangesAsync();
                        await Context.ReplyAsync(
                            $"Successfully left {(await db.GetClubAsync(club.ClubId, Context.Guild)).Name}",
                            Color.Green.RawValue);
                        status = false;
                    }
                }
                catch
                {
                    // IGNORE TODO: Add logging
                }
            }
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Summary("Leaves a club you're part of")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task LeaveClubAsync(uint id)
        {
            using (var db = new DbService())
            {
                var clubs = await db.ClubPlayers
                    .Where(x => x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id).ToListAsync();
                var club = clubs.FirstOrDefault(x => x.ClubId == id);
                if (club == null)
                {
                    await Context.ReplyAsync("You're not in a club by that ID.", Color.Red.RawValue);
                    return;
                }

                db.ClubPlayers.Remove(club);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Successfully left {(await db.GetClubAsync(club.ClubId, Context.Guild)).Name}",
                    Color.Green.RawValue);
            }
        }

        [Command("promote", RunMode = RunMode.Async)]
        [Summary("Promotes someone to a higher rank")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubPromoteAsync(IGuildUser user)
        {
            if (user.Id == Context.User.Id) return;
            using (var db = new DbService())
            {
                var leaderCheck = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                if (leaderCheck == null)
                {
                    await Context.ReplyAsync("You can't use this command as you're not a leader of any clubs",
                        Color.Red.RawValue);
                    return;
                }

                var clubUser = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                    x.GuildId == user.GuildId && x.ClubId == leaderCheck.Id && x.UserId == user.Id);
                if (clubUser == null)
                {
                    await Context.ReplyAsync($"{user.Mention} is not part of {leaderCheck.Name}", Color.Red.RawValue);
                    return;
                }

                if (await db.ClubPlayers
                        .Where(x => x.GuildId == Context.Guild.Id && x.UserId == user.Id && x.Rank < 3).CountAsync() >=
                    1)
                {
                    await Context.ReplyAsync($"{user.Mention} is already promoted in a different club",
                        Color.Red.RawValue);
                    return;
                }

                if (clubUser.Rank == 2)
                {
                    await Context.ReplyAsync(
                        $"{Context.User.Mention}, you sure you want to transfer ownership to {user.Mention}? (y/n)");
                    var status = true;
                    while (status)
                    {
                        var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(30));
                        if (response.Content.ToLower() == "n")
                        {
                            await Context.ReplyAsync("Cancelled.", Color.Green.RawValue);
                            status = false;
                        }
                        else if (response.Content.ToLower() == "y")
                        {
                            var leader =
                                await db.ClubPlayers.FindAsync(leaderCheck.Id, Context.Guild.Id, Context.User.Id);
                            leader.Rank = 2;
                            clubUser.Rank = 1;
                            await db.SaveChangesAsync();
                            await Context.ReplyAsync($"Transferred ownership of {leaderCheck.Name} to {user.Mention}",
                                Color.Green.RawValue);
                            status = false;
                        }
                    }
                }
                else if (clubUser.Rank == 3)
                {
                    clubUser.Rank = 2;
                    await Context.ReplyAsync($"Promoted {user.Mention} to rank 2",
                        Color.Green.RawValue);
                    await db.SaveChangesAsync();
                }
            }
        }

        [Command("demote", RunMode = RunMode.Async)]
        [Summary("Demotes someone to a lower rank")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubDemoteAsync(IGuildUser user)
        {
            if (user == Context.User) return;
            using (var db = new DbService())
            {
                var leader = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                var clubUser = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                    x.GuildId == user.GuildId && x.ClubId == leader.Id && x.UserId == user.Id);
                if (clubUser == null)
                {
                    await Context.ReplyAsync($"Can't demote {user.Mention} because he/she is not part of {leader.Name}",
                        Color.Red.RawValue);
                    return;
                }

                if (clubUser.Rank == 3)
                    return;
                if (clubUser.Rank == 1) return;

                clubUser.Rank = 3;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Demoted {user.Mention} down to rank 3 in {leader.Name}",
                    Color.Green.RawValue);
            }
        }

        [Command("channel", RunMode = RunMode.Async)]
        [Summary("Creates a channel and role for the club")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubChannelAsync()
        {
            using (var db = new DbService())
            {
                var leader = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                if (leader.Channel.HasValue) return;
                if (leader.RoleId.HasValue) return;
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (!cfg.ClubChannelCategory.HasValue)
                {
                    await Context.ReplyAsync("This server doesn\'t allow club channels", Color.Red.RawValue);
                    return;
                }

                var users = await db.ClubPlayers.Where(x => x.GuildId == Context.Guild.Id && x.ClubId == leader.Id)
                    .ToListAsync();
                var amount = await _clubService.IsChannelRequirementAsync(db, users);
                if (amount < 4)
                {
                    await Context.ReplyAsync(
                        "Club does not have the required amount of people that's of level 40 or higher to create a channel",
                        Color.Red.RawValue);
                    return;
                }

                try
                {
                    await _clubService.CreateChannelAsync(db, Context.Guild, cfg, leader.Name,
                        Context.User as IGuildUser,
                        users, leader);
                    await Context.ReplyAsync($"Successfully created channel for club {leader.Name} !",
                        Color.Green.RawValue);
                }
                catch (Exception e)
                {
                    await Context.ReplyAsync("Something went wrong...", Color.Red.RawValue);
                    Console.WriteLine(e);
                }
            }
        }

        [Command("list", RunMode = RunMode.Async)]
        [Alias("clubs")]
        [Summary("Paginates all clubs")]
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
                    var leader = Context.Guild.GetUser(x.Leader).Mention ??
                                 "Couldn't find user or left server.";
                    pages.Add($"**{x.Name} (id: {x.Id})**\n" +
                              $"Members: {await db.ClubPlayers.CountAsync(y => y.GuildId == Context.Guild.Id && y.ClubId == x.Id)}\n" +
                              $"Leader {leader}\n\n");
                }

                await PagedReplyAsync(pages.PaginateBuilder(Context.Guild, $"Clubs in {Context.Guild.Name}"));
            }
        }

        [Command("check", RunMode = RunMode.Async)]
        [Summary("Checks specific club information")]
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

                await Context.ReplyAsync($"**{club.Name} (ID:{club.Id}**\n" +
                                         $"Members: {await db.ClubPlayers.CountAsync(x => x.GuildId == Context.Guild.Id && x.ClubId == club.Id)}\n" +
                                         $"Leader {Context.Guild.GetUser(club.Leader).Mention ?? "Couldn't find user or left server."}");
            }
        }

        [Command("description", RunMode = RunMode.Async)]
        [Alias("desc")]
        [Summary("Sets description of a club")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubDescriptionAsync([Remainder] string content)
        {
            using (var db = new DbService())
            {
                var leader = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                if (leader == null) return;
                await Context.Message.DeleteAsync();
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                leader.Description = content;
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Updated description of club!", Color.Green.RawValue);
                if (leader.AdMessage.HasValue && cfg.ClubAdvertisementChannel.HasValue)
                {
                    var msg = await Context.Guild.GetTextChannel(cfg.ClubAdvertisementChannel.Value)
                        .GetMessageAsync(leader.AdMessage.Value) as IUserMessage;
                    await _clubService.UpdatePostAsync(cfg, msg, leader, UpdateType.Description, content);
                }
            }
        }

        [Command("image", RunMode = RunMode.Async)]
        [Alias("pic")]
        [Summary("Sets a picture to a club")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubImageAsync(string image)
        {
            using (var db = new DbService())
            {
                var leader = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                if (leader == null) return;
                await Context.Message.DeleteAsync();
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                leader.ImageUrl = image;
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Updated image to club!", Color.Green.RawValue);
                if (leader.AdMessage.HasValue && cfg.ClubAdvertisementChannel.HasValue)
                {
                    var msg = await Context.Guild.GetTextChannel(cfg.ClubAdvertisementChannel.Value)
                        .GetMessageAsync(leader.AdMessage.Value) as IUserMessage;
                    await _clubService.UpdatePostAsync(cfg, msg, leader, UpdateType.Image, image);
                }
            }
        }

        [Command("public", RunMode = RunMode.Async)]
        [Summary("Toggles a club to be public or not")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubPublicAsync()
        {
            using (var db = new DbService())
            {
                var leader = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                if (leader == null) return;
                if (leader.Public)
                {
                    leader.Public = false;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Club is no longer public. People need invite to enter the club.",
                        Color.Green.RawValue);
                }
                else
                {
                    leader.Public = true;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Set club as public. Anyone can join!", Color.Green.RawValue);
                    if (leader.AdMessage.HasValue)
                    {
                        var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                        if (cfg.ClubAdvertisementChannel.HasValue)
                        {
                            var msg = await Context.Guild.GetTextChannel(cfg.ClubAdvertisementChannel.Value)
                                .GetMessageAsync(leader.AdMessage.Value) as IUserMessage;
                            await msg.AddReactionAsync(new Emoji("\u2714"));
                        }
                    }
                }
            }
        }

        [Command("advertise", RunMode = RunMode.Async)]
        [Summary("Posts a advertisement of club to designated advertisement channel")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubAdvertiseAsync()
        {
            using (var db = new DbService())
            {
                var leader = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                if (leader == null) return;
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (!cfg.ClubAdvertisementChannel.HasValue)
                {
                    await Context.ReplyAsync("This server hasn't setup or doesn't allow club advertisement.",
                        Color.Red.RawValue);
                    return;
                }

                if (!leader.AdMessage.HasValue)
                {
                    await _clubService.PostAdvertisementAsync(db, cfg, Context.Guild, leader);
                    await Context.ReplyAsync("Posted ad!", Color.Green.RawValue);
                }
                else
                {
                    var msg = await Context.Guild.GetTextChannel(cfg.ClubAdvertisementChannel.Value)
                        .GetMessageAsync(leader.AdMessage.Value);
                    if (msg == null)
                    {
                        await _clubService.PostAdvertisementAsync(db, cfg, Context.Guild, leader);
                        await Context.ReplyAsync("Posted ad!", Color.Green.RawValue);
                    }
                    else
                    {
                        await Context.ReplyAsync("There's already an ad up!", Color.Red.RawValue);
                    }
                }
            }
        }
    }

    [Group("club settings")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    [RequireContext(ContextType.Guild)]
    public class ClubAdmin : InteractiveBase
    {
        [Command("advertisement", RunMode = RunMode.Async)]
        [Summary("Sets channel where club advertisements will be posted. \nLeave empty to disable")]
        public async Task ClubSetAdvertismentChannel(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (channel == null)
                {
                    cfg.ClubAdvertisementChannel = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Disabled Advertisement creation of clubs.",
                        Color.Green.RawValue);
                    return;
                }

                if (cfg.ClubAdvertisementChannel.HasValue && cfg.ClubAdvertisementChannel.Value == channel.Id)
                {
                    await Context.ReplyAsync($"Advertisement channel has already been set to {channel.Mention}",
                        Color.Red.RawValue);
                }
                else
                {
                    cfg.ClubAdvertisementChannel = channel.Id;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"Advertisement channel set has been been set to {channel.Mention}",
                        Color.Green.RawValue);
                }
            }
        }

        [Command("category", RunMode = RunMode.Async)]
        [Summary("Sets location in where club channels will be created. \nLeave empty to disable")]
        public async Task ClubSetCategory(ICategoryChannel category = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (cfg.ClubChannelCategory.HasValue && cfg.ClubChannelCategory.Value == category.Id)
                {
                    await Context.ReplyAsync($"Club category channel has already been set to {category.Name}",
                        Color.Red.RawValue);
                    return;
                }

                if (category == null)
                {
                    cfg.ClubChannelCategory = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Disabled club channel creation",
                        Color.Green.RawValue);
                }
                else
                {
                    cfg.ClubChannelCategory = category.Id;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"Club category channel set has been been set to {category.Name}",
                        Color.Green.RawValue);

                }
            }
        }

        [Command("level", RunMode = RunMode.Async)]
        [Summary("Sets level requirement for people to create a club")]
        public async Task ClubSetLevelRequirement(uint level)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                cfg.ClubChannelRequiredLevel = level;
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"Set required amount of club members above the level limit to create a channel to {level}",
                    Color.Green.RawValue);
            }
        }

        [Command("channel amount", RunMode = RunMode.Async)]
        [Summary("Sets amount required thats above the level requirement(club create) to create a channel")]
        public async Task ClubSetAmountRequirement(uint amount)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                cfg.ClubChannelRequiredAmount = amount;
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"Set required amount of club members above the level limit to create a channel to {amount}",
                    Color.Green.RawValue);
            }
        }

        [Command("Auto prune")]
        [Alias("autoprune", "prune")]
        [Summary("Automatically prune inactive clubs by their member count")]
        public async Task ClubAutoPruneToggle()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (cfg.ClubAutoPrune)
                {
                    cfg.ClubAutoPrune = false;
                    await Context.ReplyAsync("Disabled automatic deletion of low member count clubs with a channel.",
                        Color.Green.RawValue);
                }
                else
                {
                    cfg.ClubAutoPrune = true;
                    await Context.ReplyAsync("Enabled automatic deletion of low member count clubs with a channel.",
                        Color.Green.RawValue);
                }

                await db.SaveChangesAsync();
            }
        }
    }
}