using Discord;
using Discord.WebSocket;
using Jibril.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Jibril.Modules.Fleet.Services
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

        public bool IsClubMember(IUser user)
        {
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            return userData == null || userData.FleetName != "o";
        }
        public bool CanCreateClub(IUser user)
        {
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            return userData.Level >= 40 && userData.FleetName != "o";
        }

        public void CreateClub(IUser user, string name)
        {
            ClubDb.CreateClub(user, name);
        }
        public void DeleteClub()
        {

        }

        public async Task CreateChannel(IUser user, IGuild guild)
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
        }
        public async Task DeleteChannel(IUser user, IGuild guild)
        {
            var ch = await guild.CreateTextChannelAsync(GetClubName(user));
            await ch.DeleteAsync();
        }
        private async Task<ICategoryChannel> GetorCreateClubCategory(IGuild guild)
        {
            var cts = await guild.GetCategoriesAsync();
            var ct = cts.FirstOrDefault(x => x.Name == "club");
            if (ct != null) return ct;
            var club = await guild.CreateCategoryAsync("Club");
            return club;
        }

        public void Promote(IUser user)
        {
            var clubData = ClubDb.UserClubData(user).FirstOrDefault();
            if (clubData == null) return;
            if (clubData.Rank <= 2) return;
            ClubDb.Promote(user);
        }
        public void Demote(IUser user)
        {
            var clubData = ClubDb.UserClubData(user).FirstOrDefault();
            if (clubData == null) return;
            if (clubData.Rank >= 3) return;
            ClubDb.Demote(user);
        }
        public void PromoteLeader(IUser user)
        {
            var club = ClubDb.UserClubData(user).FirstOrDefault();
            if (club.Rank != 2 || club == null) return;
            var clubId = ClubDb.GetClubs().First(x => x.Id == club.ClubId);
            ClubDb.PromoteLeader(user, clubId.Id);
        }

        private string GetClubName(IUser user)
        {
            var club = ClubDb.UserClubData(user).FirstOrDefault();
            if (club == null) return null;
            return club.ClubName;
        }
        private IGuildUser GetClubLeader(int id)
        {
            var clubId = ClubDb.GetClubs().First(x => x.Id == id);
            return _client.GetUser(clubId.Leader) as IGuildUser;
        }
    }
}