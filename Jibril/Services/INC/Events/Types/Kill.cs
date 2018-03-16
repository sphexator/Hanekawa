using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Discord.WebSocket;
using Google.Apis.Util;
using Jibril.Services.INC.Calculate;
using Jibril.Services.INC.Data;
using Jibril.Services.INC.Database;

namespace Jibril.Services.INC.Events.Types
{
    public static class Kill
    {
        public static string KillEvent(Profile profile)
        {
            var trgt = GetTarget().FirstOrDefault();
            if (profile.Weapons.Pistol > 0 && profile.Weapons.Bullets > 0)
            {
                var pistolDamage = DamageOutput.PistolDamage(profile.Player.Stamina, profile.Player.Bleeding);
                DatabaseHungerGame.AddDamage(trgt.Userid, pistolDamage);
                var response = $"Hits {trgt.Name} with his pistol inflicting {pistolDamage} damage.";
                return response;
            }

            if (profile.Weapons.Bow > 0 && profile.Weapons.Arrows > 0)
            {
                var bowDamage = DamageOutput.BowDamage(profile.Player.Stamina, profile.Player.Bleeding);
                DatabaseHungerGame.AddDamage(trgt.Userid, bowDamage);
                var response = $"Hits {trgt.Name} with his bow inflicting {bowDamage} damage.";
                return response;
            }

            if (profile.Weapons.Axe > 0)
            {
                var axeDamage = DamageOutput.AxeDamage(profile.Player.Stamina, profile.Player.Bleeding);
                DatabaseHungerGame.AddDamage(trgt.Userid, axeDamage);
                var response = $"Hits {trgt.Name} with his axe inflicting {axeDamage} damage.";
                return response;
            }

            var fistDamage = DamageOutput.FistDamage(profile.Player.Stamina, profile.Player.Bleeding);
            DatabaseHungerGame.AddDamage(trgt.Userid, fistDamage);
            var msg = $"Hits {trgt.Name} with his fists inflicting {fistDamage} damage.";
            return msg;
        }

        private static IEnumerable<KillTarget> GetTarget()
        {
            var users = DatabaseHungerGame.GetUsers();
            var rand = new Random();
            var chosn = rand.Next(users.Count);
            var user = users[chosn];
            var result = new List<KillTarget>
            {
                new KillTarget
                {
                    Userid = user.UserId,
                    Name = user.Name
                }
            };
            return result;
        }
    }

    public class KillTarget
    {
        public string Name { get; set; }
        public ulong Userid { get; set; }

        internal object ThrowIfNull()
        {
            throw new NotImplementedException();
        }
    }
}
