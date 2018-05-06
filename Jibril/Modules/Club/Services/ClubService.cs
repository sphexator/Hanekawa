using Discord;
using Discord.WebSocket;
using Jibril.Services;
using Jibril.Services.Level.Lists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jibril.Modules.Club.Services
{
    public class ClubService
    {
        private const ulong GuildId = 339370914724446208;
        private static readonly OverwritePermissions DenyOverwrite =
            new OverwritePermissions(readMessages: PermValue.Deny);
        private static readonly OverwritePermissions AllowOverwrite =
            new OverwritePermissions(readMessages: PermValue.Allow, attachFiles: PermValue.Allow,
                embedLinks: PermValue.Allow);
        private static readonly OverwritePermissions LeaderOverwrite =
            new OverwritePermissions(manageMessages: PermValue.Allow);
        private readonly DiscordSocketClient _client;

        public ClubService(DiscordSocketClient client)
        {
            _client = client;
            _client.UserLeft += ClubCleanUp;
        }

        private Task ClubCleanUp(SocketGuildUser user)
        {
            //throw new System.NotImplementedException();
            _ = Task.Run(async () =>
            {
                var elig = IsClubMember(user);
                var leader = IsLeader(user);
                if (leader)
                {
                    //TODO: Transfer leadership to random officer or user, else delete club and channel if it has channel
                }
                if (elig)
                {
                    await LeaveClub(user);
                }
            });
            return Task.CompletedTask;
        }

        public bool IsClubMember(IGuildUser user)
        {
            var elig = ClubDb.UserClubData(user).FirstOrDefault();
            return elig != null;
        }
        public bool CanCreateClub(IGuildUser user)
        {
            var elig = ClubDb.UserClubData(user).FirstOrDefault();
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            return elig == null && userData.Level >= 40;
        }
        public bool IsLeader(IGuildUser user)
        {
            var leader = GetClubLeader(user);
            return leader == user.Id;
        }
        public bool IsOfficer(IGuildUser user)
        {
            var club = ClubDb.UserClubData(user).FirstOrDefault();
            return club.Rank <= 2;
        }

        public void CreateClub(IGuildUser user, string name)
        {
            ClubDb.CreateClub(user, name);
        }
        public void DeleteClub(IGuildUser user)
        {
            var id = GetClubId(user);
            ClubDb.DeleteClub(id);
        }

        public async Task<string> AddClubMember(IGuildUser user, IGuildUser leader)
        {
            var clubUserData = ClubDb.UserClubData(leader).FirstOrDefault();
            var clubData = ClubDb.GetClubs().FirstOrDefault(x => x.Id == clubUserData.ClubId);
            ClubDb.AddClubMember(user, clubUserData.ClubId, clubUserData.ClubName);
            if (clubData.ChannelId != 0)
            {
                await AssignRole(user, user.Guild, clubUserData.ClubName);
            }
            return clubUserData.ClubName;
        }
        public async Task<string> RemoveClubMember(IGuildUser user, IGuildUser leader)
        {
            var clubUserData = ClubDb.UserClubData(leader).FirstOrDefault();
            var clubData = ClubDb.GetClubs().FirstOrDefault(x => x.Id == clubUserData.ClubId);
            ClubDb.RemoveClubMember(user, clubUserData.ClubId);
            if (clubData.ChannelId != 0)
            {
                await RemoveRole(user, user.Guild, clubUserData.ClubName);
            }
            return clubUserData.ClubName;
        }
        public async Task<string> LeaveClub(IGuildUser user)
        {
            var clubUserData = ClubDb.UserClubData(user).FirstOrDefault();
            var clubData = ClubDb.GetClubs().FirstOrDefault(x => x.Id == clubUserData.ClubId);
            ClubDb.RemoveClubMember(user, clubUserData.ClubId);
            if (clubData.ChannelId != 0)
            {
                await RemoveRole(user, user.Guild, clubUserData.ClubName);
            }
            return clubUserData.ClubName;
        }

        public async Task<string> CreateChannel(IGuildUser user, IGuild guild)
        {
            var club = ClubDb.UserClubData(user).FirstOrDefault();
            var clubData = ClubDb.ClubData(club.ClubId);
            var users = GetClubUserDataLevel40(clubData);
            if (users.Count < 4) return "You do not have enough members of level 40 or higher";
            var elig = ClubDb.GetClubs().FirstOrDefault(x => x.Leader == user.Id);
            if (elig.ChannelId != 0) return $"{user.Nickname ?? user.Username}, you already have a channel for {club.ClubName}";
            var ct = await GetorCreateClubCategory(guild);
            var ch = await guild.CreateTextChannelAsync(club.ClubName);
            await ch.ModifyAsync(x => x.CategoryId = ct.Id);

            var role = await guild.CreateRoleAsync(club.ClubName, GuildPermissions.None);
            await ch.AddPermissionOverwriteAsync(role, AllowOverwrite);
            await ch.AddPermissionOverwriteAsync(guild.EveryoneRole, DenyOverwrite);
            await ch.AddPermissionOverwriteAsync(user, LeaderOverwrite);
            ClubDb.ChannelCreated(GetClubId(user), role.Id, ch.Id);

            var members = GetClubUserData(clubData);
            foreach (var x in members)
            {
                try
                {
                    var id = Convert.ToUInt64(x.UserId);
                    await _client.GetGuild(GuildId).GetUser(id).AddRoleAsync(role);
                }
                catch
                {
                    // ignore
                }
            }
            return $"Successfully created channel for {club.ClubName}.";
        }
        public async Task DeleteChannel(IGuildUser user, IGuild guild)
        {
            var name = GetClubName(user);
            var ch = await guild.CreateTextChannelAsync(name);
            var role = guild.Roles.First(x => x.Name == name);
            await ch.DeleteAsync();
            await role.DeleteAsync();
        }
        private async Task<ICategoryChannel> GetorCreateClubCategory(IGuild guild)
        {
            var cts = await guild.GetCategoriesAsync();
            var ct = cts.FirstOrDefault(x => x.Name == "Club");
            if (ct != null) return ct;
            var club = await guild.CreateCategoryAsync("Club");
            return club;
        }
        private async Task AssignRole(IGuildUser user, IGuild guild, string name)
        {
            var role = guild.Roles.FirstOrDefault(x => x.Name == name);
            await user.AddRoleAsync(role);
        }
        private async Task RemoveRole(IGuildUser user, IGuild guild, string name)
        {
            var role = guild.Roles.FirstOrDefault(x => x.Name == name);
            await user.RemoveRoleAsync(role);
        }

        public void Promote(IGuildUser user)
        {
            var clubData = ClubDb.UserClubData(user).FirstOrDefault();
            if (clubData == null) return;
            if (clubData.Rank <= 2) return;
            ClubDb.Promote(user);
        }
        public void Demote(IGuildUser user)
        {
            var clubData = ClubDb.UserClubData(user).FirstOrDefault();
            if (clubData == null) return;
            if (clubData.Rank >= 3) return;
            ClubDb.Demote(user);
        }
        public void PromoteLeader(IGuildUser user, IGuildUser oldLeader)
        {
            var club = ClubDb.UserClubData(user).FirstOrDefault();
            if (club.Rank != 2) return;
            var clubId = ClubDb.GetClubs().First(x => x.Id == club.ClubId);
            ClubDb.PromoteLeader(user, oldLeader, clubId.Id);
        }

        private string GetClubName(IGuildUser user)
        {
            var club = ClubDb.UserClubData(user).FirstOrDefault();
            return club?.ClubName;
        }
        private IGuildUser GetClubLeader(int id)
        {
            var clubId = ClubDb.GetClubs().First(x => x.Id == id);
            return _client.GetUser(clubId.Leader) as IGuildUser;
        }
        private ulong GetClubLeader(IGuildUser id)
        {
            var clubId = ClubDb.GetClubs().First(x => x.Leader == id.Id);
            return clubId.Leader;
        }
        private int GetClubId(IGuildUser user)
        {
            var clubId = ClubDb.GetClubs().FirstOrDefault(x => x.Leader == user.Id);
            return (int) clubId?.Id;
        }
        private IReadOnlyCollection<UserData> GetClubUserDataLevel40(IEnumerable<FleetUserInfo> clubUser)
        {
            return (from x in clubUser
                select DatabaseService.UserData(x.UserId).FirstOrDefault()
                into y
                where y.Level >= 40
                select new UserData
                {
                    UserId = y.UserId,
                    Username = y.Username,
                    Tokens = y.Tokens,
                    Event_tokens = y.Event_tokens,
                    Level = y.Level,
                    Xp = y.Xp,
                    Total_xp = y.Total_xp,
                    Daily = y.Daily,
                    Cooldown = y.Cooldown,
                    Voice_timer = y.Voice_timer,
                    JoinDateTime = y.JoinDateTime,
                    FleetName = y.FleetName,
                    ShipClass = y.ShipClass,
                    Profilepic = y.Profilepic,
                    GameCD = y.GameCD,
                    BetCD = y.BetCD,
                    Hasrole = y.Hasrole,
                    Toxicityvalue = y.Toxicityvalue,
                    Toxicitymsgcount = y.Toxicitymsgcount,
                    Toxicityavg = y.Toxicityavg,
                    Rep = y.Rep,
                    Repcd = y.Repcd,
                    FirstMsg = y.FirstMsg,
                    LastMsg = y.LastMsg
                }).ToList();
        }
        private IReadOnlyCollection<UserData> GetClubUserData(IEnumerable<FleetUserInfo> clubUser)
        {
            return clubUser.Select(x => DatabaseService.UserData(x.UserId).FirstOrDefault())
                .Select(y => new UserData
                {
                    UserId = y.UserId,
                    Username = y.Username,
                    Tokens = y.Tokens,
                    Event_tokens = y.Event_tokens,
                    Level = y.Level,
                    Xp = y.Xp,
                    Total_xp = y.Total_xp,
                    Daily = y.Daily,
                    Cooldown = y.Cooldown,
                    Voice_timer = y.Voice_timer,
                    JoinDateTime = y.JoinDateTime,
                    FleetName = y.FleetName,
                    ShipClass = y.ShipClass,
                    Profilepic = y.Profilepic,
                    GameCD = y.GameCD,
                    BetCD = y.BetCD,
                    Hasrole = y.Hasrole,
                    Toxicityvalue = y.Toxicityvalue,
                    Toxicitymsgcount = y.Toxicitymsgcount,
                    Toxicityavg = y.Toxicityavg,
                    Rep = y.Rep,
                    Repcd = y.Repcd,
                    FirstMsg = y.FirstMsg,
                    LastMsg = y.LastMsg
                })
                .ToList();
        }
    }
}