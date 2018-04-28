using Discord;
using Discord.WebSocket;
using Jibril.Services;
using System.Linq;

namespace Jibril.Modules.Fleet.Services
{
    public class ClubService
    {
        private readonly DiscordSocketClient _client;

        private static readonly OverwritePermissions DenyOverwrite = new OverwritePermissions(readMessages: PermValue.Deny);
        private static readonly OverwritePermissions AllowOverwrite = new OverwritePermissions(readMessages: PermValue.Allow, attachFiles: PermValue.Allow, embedLinks: PermValue.Allow);
        private static readonly OverwritePermissions LeaderOverwrite = new OverwritePermissions(manageMessages: PermValue.Allow);

        public ClubService(DiscordSocketClient client)
        {
            _client = client;
        }

        public bool IsClubMember(IUser user)
        {
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            return userData == null || userData.FleetName != "o";
        }
        public bool CanCreateFleet(IUser user)
        {
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            return userData.Level >= 40 && userData.FleetName != "o";
        }

        public void Createfleet(IUser user, string name)
        {
            ClubDb.CreateClub(user, name);
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
    }
}