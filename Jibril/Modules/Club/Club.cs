using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Modules.Club.Services;
using Jibril.Preconditions;
using Jibril.Services;
using Jibril.Services.Common;

namespace Jibril.Modules.Club
{
    [Group("club")]
    public class Club : InteractiveBase
    {
        private readonly ClubService _service;

        public Club(ClubService service)
        {
            _service = service;
        }

        [Command("create")]
        [Summary("Creates a fleet")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task CreateClub([Remainder] string name = null)
        {
            if (name == null) return;
            var eligible = _service.CanCreateClub(Context.User as IGuildUser);
            if (eligible != true) return;
            _service.CreateClub(Context.User as IGuildUser, name);
            await ReplyAsync($"{Context.User.Username} Successfully created club {name}");
        }

        [Command("add", RunMode = RunMode.Async)]
        [Summary("Adds a member to your fleet")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task AddClubMember(SocketGuildUser member)
        {
            await _service.AddClubMember(member, Context.User as IGuildUser);
            //TODO Make AddClubMember return string of clubname as reference for reply.
            await ReplyAsync($"Successfully added {member.Nickname ?? member.Username} to ...");
        }

        [Command("remove", RunMode = RunMode.Async)]
        [Alias("kick")]
        [Summary("Removes a user from your fleet")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task RemoveClubMember(IUser member)
        {
            //TODO Make RemoveClubMember return string of clubname as reference for reply.
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Summary("Leaves a fleet you're a part of")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task LeaveClub()
        {
            //TODO Make RemoveClubMember return string of clubname as reference for reply.
        }

        [Command("disband", RunMode = RunMode.Async)]
        [Summary("Disbands the club")]
        [RequiredChannel(339383206669320192)]
        public async Task DisbandClub()
        {

        }
    }
}