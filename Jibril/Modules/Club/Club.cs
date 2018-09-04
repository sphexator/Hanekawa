using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Preconditions;
using Hanekawa.Services.Club;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Entities.Tables;
using Microsoft.EntityFrameworkCore;
using static Hanekawa.Services.Club.ClubService;

namespace Hanekawa.Modules.Club
{
    [Group("club")]
    [RequireContext(ContextType.Guild)]
    [RequireOwner]
    public class Club : InteractiveBase
    {
        private readonly ClubService _clubService;

        public Club(ClubService clubService)
        {
            _clubService = clubService;
        }

        //TODO: Club, do this.
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
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("You do not meet the requirement to make a club (Level 40).",
                            Color.Red.RawValue).Build());
                    return;
                }

                var leaderCheck = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                if (leaderCheck != null)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("You're already a leader of a club, you can't create multiple clubs.",
                            Color.Red.RawValue).Build());
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
                    Id = (await db.ClubPlayers.CountAsync()) + 1
                };
                await db.ClubPlayers.AddAsync(data);
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Successfully created club {name} !", Color.Green.RawValue).Build());
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
                        x.GuildId == Context.Guild.Id && x.UserId == Context.Guild.Id && x.Rank <= 2);
                if (clubUser == null)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder()
                            .Reply("You're not high enough rank to use that command!", Color.Red.RawValue).Build());
                    return;
                }

                var clubData = await db.GetClubAsync(clubUser.ClubId, Context.Guild);
                var data = new ClubPlayer
                {
                    ClubId = clubUser.ClubId,
                    GuildId = Context.Guild.Id,
                    JoinDate = DateTimeOffset.UtcNow,
                    Rank = 3,
                    UserId = user.Id,
                    Id = (await db.ClubPlayers.CountAsync()) + 1
                };
                await db.ClubPlayers.AddAsync(data);
                await db.SaveChangesAsync();
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().Reply($"Added {user.Mention} to {clubData.Name}", Color.Green.RawValue).Build(),
                    TimeSpan.FromSeconds(15));
            }
        }

        [Command("remove", RunMode = RunMode.Async)]
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
                    await ReplyAsync(null, false,
                        new EmbedBuilder()
                            .Reply($"Can't remove {user.Mention} because he/she is not part of {leader.Name}",
                                Color.Red.RawValue)
                            .Build());
                    return;
                }

                db.ClubPlayers.Remove(clubUser);
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Removed {user.Mention} from {leader.Name}", Color.Green.RawValue)
                        .Build());
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

                var embed = new EmbedBuilder().Reply(content);
                embed.Title = "Reply with the ID of club you wish to leave";
                await ReplyAsync(null, false, embed.Build());
                var status = true;
                while (status)
                {
                    var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(30));
                    if (!int.TryParse(response.Content, out var result)) continue;
                    var club = clubs.FirstOrDefault(x => x.ClubId == result);
                    if (club == null) continue;
                    db.ClubPlayers.Remove(club);
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder()
                            .Reply($"Successfully left {(await db.GetClubAsync(club.ClubId, Context.Guild)).Name}",
                                Color.Green.RawValue).Build());
                    status = false;
                }
            }
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Summary("Leaves a club you're part of")]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task LeaveClubAsync(uint id)
        {
            using (var db = new DbService())
            {
                var clubs = await db.ClubPlayers
                    .Where(x => x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id).ToListAsync();
                var club = clubs.FirstOrDefault(x => x.ClubId == id);
                if (club == null)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("You're not in a club by that ID.", Color.Red.RawValue).Build());
                    return;
                }

                db.ClubPlayers.Remove(club);
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"Successfully left {(await db.GetClubAsync(club.ClubId, Context.Guild)).Name}",
                            Color.Green.RawValue).Build());
            }
        }

        [Command("promote", RunMode = RunMode.Async)]
        [Summary("Promotes someone to a higher rank")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubPromoteAsync(IGuildUser user)
        {
            if (user == Context.User) return;
            using (var db = new DbService())
            {
                var leaderCheck = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                if (leaderCheck == null)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("You can't use this command as you're not a leader of any clubs",
                            Color.Red.RawValue).Build());
                    return;
                }

                var clubUser = await db.ClubPlayers.FindAsync(Context.Guild.Id, leaderCheck.Id, user.Id);
                if (clubUser == null)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder()
                            .Reply($"{user.Mention} is not part of {leaderCheck.Name}", Color.Red.RawValue).Build());
                    return;
                }

                if (await db.ClubPlayers
                        .Where(x => x.GuildId == Context.Guild.Id && x.UserId == user.Id && x.Rank <= 3).CountAsync() >=
                    1)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"{user.Mention} is already promoted in a different club",
                            Color.Red.RawValue).Build());
                    return;
                }

                if (clubUser.Rank == 2)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder()
                            .Reply(
                                $"{Context.User.Mention}, you sure you want to transfer ownership to {user.Mention}? (y/n)")
                            .Build());
                    var status = true;
                    while (status)
                    {
                        var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(30));
                        if (response.Content.ToLower() == "n")
                        {
                            await ReplyAsync(null, false,
                                new EmbedBuilder().Reply("Cancelled.", Color.Green.RawValue).Build());
                            status = false;
                        }
                        else if (response.Content.ToLower() == "y")
                        {
                            var leader =
                                await db.ClubPlayers.FindAsync(Context.Guild.Id, leaderCheck.Id, Context.User.Id);
                            leader.Rank = 2;
                            clubUser.Rank = 1;
                            await db.SaveChangesAsync();
                            await ReplyAsync(null, false,
                                new EmbedBuilder()
                                    .Reply($"Transferred ownership of {leaderCheck.Name} to {user.Mention}",
                                        Color.Green.RawValue).Build());
                            status = false;
                        }
                    }
                }
                else if (clubUser.Rank == 3)
                {
                    clubUser.Rank = 2;
                    await ReplyAsync(null, false,
                        new EmbedBuilder()
                            .Reply($"Promoted {user.Mention} to rank 2",
                                Color.Green.RawValue).Build());
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
                var clubUser = await db.ClubPlayers.FindAsync(Context.User.Id, Context.Guild.Id, leader.Id);
                if (clubUser == null)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder()
                            .Reply($"Can't demote {user.Mention} because he/she is not part of {leader.Name}",
                                Color.Red.RawValue)
                            .Build());
                    return;
                }

                if (clubUser.Rank == 3)
                    return;
                if (clubUser.Rank == 1) return;

                clubUser.Rank = 3;
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply($"Demoted {user.Mention} down to rank 3 in {leader.Name}",
                        Color.Green.RawValue).Build());
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
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"This server doesn't allow club channels", Color.Red.RawValue)
                            .Build());
                    return;
                }

                var users = await db.ClubPlayers.Where(x => x.GuildId == Context.Guild.Id && x.ClubId == leader.Id)
                    .ToListAsync();
                var amount = await _clubService.IsChannelRequirementAsync(db, users);
                if (amount < 4)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder()
                            .Reply(
                                "Club does not have the required amount of people that's of level 40 or higher to create a channel",
                                Color.Red.RawValue).Build());
                    return;
                }

                try
                {
                    await _clubService.CreateChannelAsync(db, Context.Guild, cfg, leader.Name, Context.User as IGuildUser,
                        users, leader);
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Successfully created channel for club {leader.Name} !",
                            Color.Green.RawValue).Build());
                }
                catch (Exception e)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Something went wrong...", Color.Red.RawValue).Build());
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
                var pages = new List<string>();
                for (var i = 0; i < clubs.Count;)
                {
                    string clubString = null;
                    for (var j = 0; j < 5; j++)
                    {
                        if (i == clubs.Count) continue;
                        var club = clubs[i];
                        var leader = Context.Guild.GetUser(club.Leader).Mention ?? "Couldn't find user or left server.";
                        clubString += $"**{club.Name} (ID:{club.Id}**\n" +
                                      $"Members: {await db.ClubPlayers.CountAsync(x => x.GuildId == Context.Guild.Id && x.ClubId == club.Id)}\n" +
                                      $"Leader {leader}";
                        i++;
                    }

                    pages.Add(clubString);
                }

                var paginator = new PaginatedMessage
                {
                    Color = Color.Purple,
                    Pages = pages,
                    Title = $"Clubs in {Context.Guild.Name}",
                    Options = new PaginatedAppearanceOptions
                    {
                        First = new Emoji("⏮"),
                        Back = new Emoji("◀"),
                        Next = new Emoji("▶"),
                        Last = new Emoji("⏭"),
                        Stop = null,
                        Jump = null,
                        Info = null
                    }
                };
                await PagedReplyAsync(paginator);
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
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Couldn't find a club with that ID.", Color.Red.RawValue).Build());
                    return;
                }

                await ReplyAsync(
                    null,
                    false,
                    new EmbedBuilder()
                        .Reply($"**{club.Name} (ID:{club.Id}**\n" +
                               $"Members: {await db.ClubPlayers.CountAsync(x => x.GuildId == Context.Guild.Id && x.ClubId == club.Id)}\n" +
                               $"Leader {Context.Guild.GetUser(club.Leader).Mention ?? "Couldn't find user or left server."}")
                        .Build());
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
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply("Updated description of club!", Color.Green.RawValue).Build());
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
                await ReplyAsync(null, false,
                    new EmbedBuilder().Reply("Updated image to club!", Color.Green.RawValue).Build());
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
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Club is no longer public. People need invite to enter the club.",
                            Color.Green.RawValue).Build());
                }
                else
                {
                    leader.Public = true;
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Set club as public. Anyone can join!", Color.Green.RawValue).Build());
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
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("This server hasn't setup or doesn't allow club advertisement.",
                            Color.Red.RawValue).Build());
                    return;
                }

                if (!leader.AdMessage.HasValue)
                {
                    await _clubService.PostAdvertisementAsync(db, cfg, Context.Guild, leader);
                    await ReplyAsync(null, false, new EmbedBuilder().Reply("Posted ad!", Color.Green.RawValue).Build());
                }
                else
                {
                    var msg = await Context.Guild.GetTextChannel(cfg.ClubAdvertisementChannel.Value)
                        .GetMessageAsync(leader.AdMessage.Value);
                    if (msg == null)
                    {
                        await _clubService.PostAdvertisementAsync(db, cfg, Context.Guild, leader);
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply("Posted ad!", Color.Green.RawValue).Build());
                    }
                    else
                    {
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply("There's already an ad up!", Color.Red.RawValue).Build());
                    }
                }
            }
        }
    }

    [Group("club settings")]
    [RequireContext(ContextType.Guild)]
    [RequireOwner]
    public class ClubAdmin : InteractiveBase
    {
        [Command("advertisement", RunMode = RunMode.Async)]
        [Summary("Sets channel where club advertisements will be posted. \nLeave empty to disable")]
        public async Task ClubSetAdvertismentChannel(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                if (cfg.ClubAdvertisementChannel.HasValue && cfg.ClubAdvertisementChannel.Value == channel.Id)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Advertisement channel has already been set to {channel.Mention}",
                            Color.Red.RawValue).Build());
                    return;
                }

                if (channel == null)
                {
                    cfg.ClubAdvertisementChannel = null;
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Disabled Advertisement creation of clubs.",
                            Color.Green.RawValue).Build());
                }
                else
                {
                    cfg.ClubAdvertisementChannel = channel.Id;
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Advertisement channel set has been been set to {channel.Mention}",
                            Color.Green.RawValue).Build());
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
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Club category channel has already been set to {category.Name}",
                            Color.Red.RawValue).Build());
                    return;
                }

                if (category == null)
                {
                    cfg.ClubChannelCategory = null;
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("Disabled club channel creation",
                            Color.Green.RawValue).Build());
                }
                else
                {
                    cfg.ClubChannelCategory = category.Id;
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Club category channel set has been been set to {category.Name}",
                            Color.Green.RawValue).Build());
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
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply(
                            $"Set required amount of club members above the level limit to create a channel to {level}",
                            Color.Green.RawValue).Build());
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
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply(
                            $"Set required amount of club members above the level limit to create a channel to {amount}",
                            Color.Green.RawValue).Build());
            }
        }
    }
}