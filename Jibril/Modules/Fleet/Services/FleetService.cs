﻿using Discord;
using Jibril.Services;
using System.Linq;

namespace Jibril.Modules.Fleet.Services
{
    public class FleetService
    {
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
            FleetDb.CreateFleet(user, name);
        }
    }
}