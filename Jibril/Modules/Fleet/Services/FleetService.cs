using Discord;
using Jibril.Services;
using System.Linq;
using Discord.WebSocket;

namespace Jibril.Modules.Fleet.Services
{
    public class FleetService
    {
        private readonly DiscordSocketClient _client;
        public FleetService(DiscordSocketClient client)
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
            FleetDb.CreateClub(user, name);
        }

        public void Promote(IUser user)
        {
            FleetDb.Promote(user);
        }
        public void Demote(IUser user)
        {
            FleetDb.Demote(user);
        }
    }
}