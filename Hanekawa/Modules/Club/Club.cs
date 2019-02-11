using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Modules.Club.Handler;
using Hanekawa.Preconditions;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Club
{
    [RequireContext(ContextType.Guild)]
    [Name("Club")]
    public class Club : InteractiveBase
    {
        private readonly Search _search;
        private readonly Admin _admin;
        private readonly Advertise _advertise;

        public Club(Admin admin, Search search, Advertise advertise)
        {
            _admin = admin;
            _search = search;
            _advertise = advertise;
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
        public async Task AddClubMemberAsync(IGuildUser user) => await _admin.AddAsync(Context, user);

        [Name("Club remove")]
        [Command("club remove", RunMode = RunMode.Async)]
        [Alias("club kick")]
        [Summary("Removes a user from your club")]
        [Remarks("h.club remove @bob#0000")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task RemoveClubMemberAsync(IGuildUser user) => await _admin.RemoveAsync(Context, user);

        [Name("Club leave")]
        [Command("club leave", RunMode = RunMode.Async)]
        [Summary("Leaves a club you're part of")]
        [Remarks("h.club leave")]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task LeaveClubAsync() => await _admin.LeaveAsync(Context);

        [Name("Club promote")]
        [Command("club promote", RunMode = RunMode.Async)]
        [Summary("Promotes someone to a higher rank")]
        [Remarks("h.club promote @bob#0000")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubPromoteAsync(IGuildUser user) => await _admin.PromoteAsync(Context, user);

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
        public async Task ClubChannelAsync() => await _admin.CreateChannelAsync(Context);

        [Name("Club list")]
        [Command("club list", RunMode = RunMode.Async)]
        [Alias("club clubs")]
        [Summary("Paginates all clubs")]
        [Remarks("h.club list")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubListAsync() => await _search.ClubListAsync(Context);

        [Name("Club check")]
        [Command("club check", RunMode = RunMode.Async)]
        [Summary("Checks specific club information")]
        [Remarks("h.club check 15")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task ClubCheckAsync(int id) => await _search.Check(Context, id);

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
        public async Task BlackListUser(IGuildUser user, [Remainder] string reason = null) =>
            await _admin.BlackListUserAsync(Context, user, reason);

        [Name("Club blacklist")]
        [Command("club blacklist", RunMode = RunMode.Async)]
        [Alias("cb")]
        [Summary("Gets current blacklist for their club")]
        [Remarks("h.cb")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel]
        public async Task GetBlackList() =>
            await _admin.GetBlackListAsync(Context);
    }
}