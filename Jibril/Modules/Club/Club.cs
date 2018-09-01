using System;
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

namespace Hanekawa.Modules.Club
{
    [Group("club")]
    [RequireContext(ContextType.Guild)]
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
        public async Task CreateClub([Remainder]string name)
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

                var leaderCheck = db.ClubInfos.Where(x => x.GuildId == Context.Guild.Id && x.Leader == Context.User.Id);
                if (leaderCheck.FirstOrDefault(x => x.Leader == Context.User.Id) != null)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply("You're already a leader of a club, you can't create multiple clubs.",
                            Color.Red.RawValue).Build());
                    return;
                }

                await db.CreateClub(Context.User, Context.Guild, name, DateTimeOffset.UtcNow);
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
            using (var db = new DbService())
            {
                var clubUser =
                    await db.ClubPlayers.Where(x => x.GuildId == Context.Guild.Id && x.UserId == Context.Guild.Id).FirstOrDefaultAsync(x => x.Rank <= 2);
                if (clubUser == null)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder()
                            .Reply($"You're not high enough rank to use that command!", Color.Red.RawValue).Build());
                    return;
                }

                var clubData = await db.GetClubAsync(clubUser.ClubId, Context.Guild);
                var data = new ClubPlayer
                {
                    ClubId = clubUser.ClubId,
                    GuildId = Context.Guild.Id,
                    JoinDate = DateTimeOffset.UtcNow,
                    Rank = 3,
                    UserId = user.Id
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
            using (var db = new DbService())
            {

            }
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Summary("Leaves a club you're part of")]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task LeaveClubAsync()
        {
            using (var db = new DbService())
            {

            }
        }

        [Command("promote", RunMode = RunMode.Async)]
        [Summary("Promotes someone to a higher rank")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubPromoteAsync(IGuildUser user)
        {
            using (var db = new DbService())
            {

            }
        }

        [Command("demote", RunMode = RunMode.Async)]
        [Summary("Demotes someone to a lower rank")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubDemoteAsync(IGuildUser user)
        {
            using (var db = new DbService())
            {

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

            }
        }
    }
}
