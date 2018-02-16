using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using Jibril.Services.INC.Calculate;
using Jibril.Services.INC.Data;
using Jibril.Services.INC.Database;

namespace Jibril.Services.INC.Events.Types
{
    public static class Kill
    {
        public static string KillEvent(Profile profile)
        {
            var trgt = GetTarget();
            if (profile.Weapons.Pistol > 0 && profile.Weapons.Bullets > 0)
            {
                var pistolDamage = DamageOutput.PistolDamage(profile.Player.Stamina, profile.Player.Bleeding);
                var response = $"Hits {trgt} with his axe inflicting {pistolDamage} damage.";
                return response;
            }

            if (profile.Weapons.Bow > 0 && profile.Weapons.Arrows > 0)
            {
                var bowDamage = DamageOutput.BowDamage(profile.Player.Stamina, profile.Player.Bleeding);
                //DatabaseHungerGame.AddDamage();
                var response = $"Hits {trgt} with his axe inflicting {bowDamage} damage.";
                return response;
            }

            if (profile.Weapons.Axe > 0)
            {
                var axeDamage = DamageOutput.AxeDamage(profile.Player.Stamina, profile.Player.Bleeding);
                var response = $"Hits {trgt} with his axe inflicting {axeDamage} damage.";
                return response;
            }

            var fistDamage = DamageOutput.FistDamage(profile.Player.Stamina, profile.Player.Bleeding);
            var msg = $"Hits {trgt} with his axe inflicting {fistDamage} damage.";
            return msg;
        }

        private static ulong GetTarget()
        {
            var users = DatabaseHungerGame.GetUsers();
            var rand = new Random();
            var chosn = rand.Next(users.Count);
            var user = users[chosn].UserId;
            return user;
        }
    }
}
