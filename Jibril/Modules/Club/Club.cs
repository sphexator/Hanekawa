using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Preconditions;
using Hanekawa.Services.Club;

namespace Hanekawa.Modules.Club
{
    [Group("club")]
    public class Club : InteractiveBase
    {
        private readonly ClubService _clubService;
        public Club(ClubService clubService)
        {
            _clubService = clubService;
        }
        //TODO: Club, do this.
        [Command("club", RunMode = RunMode.Async)]
        [Summary("Creates a club")]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task CreateClub()
        {

        }

        [Command("add", RunMode = RunMode.Async)]
        [Summary("Adds a member to your club")]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task AddClubMemberAsync()
        {

        }

        [Command("remove", RunMode = RunMode.Async)]
        [Summary("Removes a user from your club")]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task RemoveClubMemberAsync()
        {

        }

        [Command("leave", RunMode = RunMode.Async)]
        [Summary("Leaves a club you're part of")]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task LeaveClubAsync()
        {

        }

        [Command("promote", RunMode = RunMode.Async)]
        [Summary("Promotes someone to a higher rank")]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task ClubPromoteAsync()
        {

        }

        [Command("demote", RunMode = RunMode.Async)]
        [Summary("Demotes someone to a lower rank")]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task ClubDemoteAsync()
        {

        }

        [Command("channel", RunMode = RunMode.Async)]
        [Summary("Creates a channel and role for the club")]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task ClubChannelAsync()
        {

        }

        [Command("list", RunMode = RunMode.Async)]
        [Alias("clubs")]
        [Summary("Paginates all clubs")]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task ClubListAsync()
        {

        }

        [Command("check", RunMode = RunMode.Async)]
        [Summary("Checks specific club information")]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task ClubCheckAsync()
        {
            
        }
    }
}
