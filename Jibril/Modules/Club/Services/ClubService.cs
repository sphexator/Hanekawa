using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Jibril.Services;

namespace Jibril.Modules.Club.Services
{
    public class ClubService
    {
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
        }

        public bool IsClubMember(IGuildUser user)
        {
            var elig = ClubDb.UserClubData(user).FirstOrDefault();
            return elig != null;
        }
        public static bool CanCreateClub(IGuildUser user)
        {
            var elig = ClubDb.UserClubData(user).FirstOrDefault();
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            return elig == null && userData.Level >= 40;
        }
        public static bool IsLeader(IGuildUser user)
        {
            var leader = GetClubLeader(user);
            return leader == user.Id;
        }
        public static bool IsOfficer(IGuildUser user)
        {
            var club = ClubDb.UserClubData(user).FirstOrDefault();
            return club.Rank <= 2;
        }

        public static void CreateClub(IGuildUser user, string name)
        {
            ClubDb.CreateClub(user, name);
        }
        public void DeleteClub(IGuildUser user)
        {
            var id = GetClubId(user);
            ClubDb.DeleteClub(id);
        }

        public static async Task<string> AddClubMember(IGuildUser user, IGuildUser leader)
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
        public static async Task<string> RemoveClubMember(IGuildUser user, IGuildUser leader)
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
        public static async Task<string> LeaveClub(IGuildUser user)
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

        public async Task CreateChannel(IGuildUser user, IGuild guild)
        {
            var clubName = GetClubName(user);
            if (clubName == null) return;

            var ct = await GetorCreateClubCategory(guild);
            var ch = await guild.CreateTextChannelAsync(clubName);
            await ch.ModifyAsync(x => x.CategoryId = ct.CategoryId);

            var role = await guild.CreateRoleAsync(clubName, GuildPermissions.None);

            await ch.AddPermissionOverwriteAsync(role, AllowOverwrite);
            await ch.AddPermissionOverwriteAsync(guild.EveryoneRole, DenyOverwrite);
            await ch.AddPermissionOverwriteAsync(user, LeaderOverwrite);
            ClubDb.ChannelCreated(GetClubId(user), role.Id, ch.Id);
        }
        public async Task DeleteChannel(IGuildUser user, IGuild guild)
        {
            var name = GetClubName(user);
            var ch = await guild.CreateTextChannelAsync(name);
            var role = guild.Roles.First(x => x.Name == name);
            await ch.DeleteAsync();
            await role.DeleteAsync();
        }
        private static async Task<ICategoryChannel> GetorCreateClubCategory(IGuild guild)
        {
            var cts = await guild.GetCategoriesAsync();
            var ct = cts.FirstOrDefault(x => x.Name == "club");
            if (ct != null) return ct;
            var club = await guild.CreateCategoryAsync("Club");
            return club;
        }
        private static async Task AssignRole(IGuildUser user, IGuild guild, string name)
        {
            var role = guild.Roles.FirstOrDefault(x => x.Name == name);
            await user.AddRoleAsync(role);
        }
        private static async Task RemoveRole(IGuildUser user, IGuild guild, string name)
        {
            var role = guild.Roles.FirstOrDefault(x => x.Name == name);
            await user.RemoveRoleAsync(role);
        }

        public static void Promote(IGuildUser user)
        {
            var clubData = ClubDb.UserClubData(user).FirstOrDefault();
            if (clubData == null) return;
            if (clubData.Rank <= 2) return;
            ClubDb.Promote(user);
        }
        public static void Demote(IGuildUser user)
        {
            var clubData = ClubDb.UserClubData(user).FirstOrDefault();
            if (clubData == null) return;
            if (clubData.Rank >= 3) return;
            ClubDb.Demote(user);
        }
        public void PromoteLeader(IGuildUser user)
        {
            var club = ClubDb.UserClubData(user).FirstOrDefault();
            if (club.Rank != 2) return;
            var clubId = ClubDb.GetClubs().First(x => x.Id == club.ClubId);
            ClubDb.PromoteLeader(user, clubId.Id);
        }

        private static string GetClubName(IGuildUser user)
        {
            var club = ClubDb.UserClubData(user).FirstOrDefault();
            return club?.ClubName;
        }
        private IGuildUser GetClubLeader(int id)
        {
            var clubId = ClubDb.GetClubs().First(x => x.Id == id);
            return _client.GetUser(clubId.Leader) as IGuildUser;
        }
        private static ulong GetClubLeader(IGuildUser id)
        {
            var clubId = ClubDb.GetClubs().First(x => x.Leader == id.Id);
            return clubId.Leader;
        }
        private int GetClubId(IGuildUser user)
        {
            var clubId = ClubDb.GetClubs().FirstOrDefault(x => x.Leader == user.Id);
            return (int) clubId?.Id;
        }
    }
}