using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Data.Variables;
using Jibril.Modules.Club.Services;
using Jibril.Preconditions;
using Jibril.Services.Common;

namespace Jibril.Modules.Club
{
    [Group("club")]
    public class Club : InteractiveBase
    {
        private const ulong ChannelId = 426964780570640386;
        private readonly ClubService _service;

        public Club(ClubService service)
        {
            _service = service;
        }

        [Command("create", RunMode = RunMode.Async)]
        [Summary("Creates a club")]
        [RequiredChannel(ChannelId)]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task CreateClub([Remainder] string name = null)
        {
            if (name == null) return;
            var eligible = _service.CanCreateClub(Context.User as IGuildUser);
            if (eligible != true)
            {
                await ReplyAsync($"{Context.User.Username}, you do not have the required permission to create a club.");
                return;
            }

            _service.CreateClub(Context.User as IGuildUser, name);
            await ReplyAsync($"{Context.User.Username} Successfully created club {name}");
        }

        [Command("add", RunMode = RunMode.Async)]
        [Summary("Adds a member to your club")]
        [RequiredChannel(ChannelId)]
        [Ratelimit(1, 1, Measure.Seconds)]
        public async Task AddClubMember(IGuildUser member)
        {
            var elig = _service.IsClubMember(member);
            if (Context.User.Id == member.Id || elig) return;

            var eligible = _service.IsOfficer(Context.User as IGuildUser);
            if (eligible != true)
            {
                await ReplyAsync(
                    $"{Context.User.Username}, you do not have the required permission to add club members.");
                return;
            }

            var name = ClubDb.UserClubData(Context.User).FirstOrDefault();
            await ReplyAsync($"{member.Mention}, do you want to join {name.ClubName} ? (Y / N)");
            var response =
                await NextMessageAsync(new EnsureFromUserCriterion(member.Id), TimeSpan.FromSeconds(60));
            if (response.Content.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
            {
                var club = await _service.AddClubMember(member, Context.User as IGuildUser);
                await ReplyAsync($"Successfully added {member.Nickname ?? member.Username} to {club}");
            }
            else
            {
                await ReplyAsync("User didn't reply in time or declined the invite.");
            }
        }

        [Command("remove", RunMode = RunMode.Async)]
        [Alias("kick")]
        [Summary("Removes a user from your club")]
        [RequiredChannel(ChannelId)]
        [Ratelimit(1, 1, Measure.Seconds)]
        public async Task RemoveClubMember(IGuildUser member)
        {
            if (member == Context.User) return;
            var aUser = ClubDb.UserClubData(Context.User).FirstOrDefault();
            var bUser = ClubDb.UserClubData(member).FirstOrDefault();
            if (bUser == null) return;
            if (aUser == null) return;
            if (aUser.ClubId != bUser.ClubId) return;

            var eligible = _service.IsLeader(Context.User as IGuildUser);
            if (eligible != true)
            {
                await ReplyAsync(
                    $"{Context.User.Username}, you do not have the required permission to kick club members.");
                return;
            }

            var club = await _service.RemoveClubMember(member, Context.User as IGuildUser);
            await ReplyAsync($"Successfully removed {member.Nickname ?? member.Username} from {club}");
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Summary("Leaves a fleet you're a part of")]
        [RequiredChannel(ChannelId)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task LeaveClub()
        {
            var club = await _service.LeaveClub(Context.User as IGuildUser);
            await ReplyAsync($"{Context.User.Username} left {club}");
        }

        [Command("promote", RunMode = RunMode.Async)]
        [Summary("Promotes someone to a higher rank")]
        [RequiredChannel(ChannelId)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task Promote(IGuildUser user)
        {
            if (user == Context.User) return;
            var aUser = ClubDb.UserClubData(Context.User).FirstOrDefault();
            var bUser = ClubDb.UserClubData(user).FirstOrDefault();
            var elig = _service.IsLeader(Context.User as IGuildUser);
            if (elig && aUser.ClubId == bUser.ClubId && bUser.Rank == 2)
            {
                await ReplyAsync(
                    $"{Context.User.Username}, you sure you want to transfer leadership to {user.Nickname ?? user.Username}? (Y/N)");
                var response = await NextMessageAsync();
                if (response.Content.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                    _service.PromoteLeader(user);
            }

            if (elig && aUser.ClubId == bUser.ClubId && bUser.Rank > 2)
            {
                _service.Promote(user);
                await ReplyAsync($"{Context.User.Username} promoted {user.Nickname ?? user.Username} to rank 2");
                return;
            }

            await ReplyAsync("Can't promote a user thats not in the same club or already rank 2");
        }

        [Command("demote", RunMode = RunMode.Async)]
        [Summary("Demotes someone to a lower rank")]
        [RequiredChannel(ChannelId)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task Demote(IGuildUser user)
        {
            if (user == Context.User) return;
            var aUser = ClubDb.UserClubData(Context.User).FirstOrDefault();
            var bUser = ClubDb.UserClubData(user).FirstOrDefault();
            var elig = _service.IsLeader(Context.User as IGuildUser);
            if (elig && aUser.ClubId == bUser.ClubId && bUser.Rank == 2)
            {
                _service.Demote(user);
                await ReplyAsync($"{Context.User.Username} demote {user.Nickname ?? user.Username} to rank 3");
                return;
            }

            await ReplyAsync("Can't demote a user thats not in the same club or already rank 2");
        }

        [Command("channel", RunMode = RunMode.Async)]
        [Summary("Creates a channel and role for the club")]
        [RequiredChannel(ChannelId)]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task ChannelCreation()
        {
            var elig = _service.IsLeader(Context.User as IGuildUser);
            if (elig == false) return;
            var response = await _service.CreateChannel(Context.User as IGuildUser, Context.Guild);
            await ReplyAsync(response);
        }

        [Command("list", RunMode = RunMode.Async)]
        [Alias("clubs")]
        [Summary("Creates a channel and role for the club")]
        [RequiredChannel(ChannelId)]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task Clubs()
        {
            var clubs = ClubDb.GetClubs();
            var pages = (from x in clubs
                let leader = Context.Guild.GetUser(x.Leader)
                select $"**{x.Name}({x.Id})**\n" + $"Members: {x.Members}\n" +
                       $"Leader: {leader.Mention}").ToList();

            await PagedReplyAsync(pages);
        }

        [Command("club", RunMode = RunMode.Async)]
        [Alias("clubs")]
        [Summary("Creates a channel and role for the club")]
        [RequiredChannel(ChannelId)]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task ClubTask(int id)
        {
            var clubs = ClubDb.GetClubs().FirstOrDefault(x => x.Id == id);
            if (clubs == null) return;
            var leader = Context.Guild.GetUser(clubs.Leader);
            var embed = EmbedGenerator.DefaultEmbed($"**{clubs.Name}({clubs.Id}**\n" +
                                                    $"Members: {clubs.Members}\n" +
                                                    $"Leader: {leader.Mention}", Colours.DefaultColour);

            await ReplyAsync(null, false, embed.Build());
        }
    }
}