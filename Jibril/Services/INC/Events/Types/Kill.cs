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
            if (profile.Weapons.Pistol > 0)
            {
                var pistolDamage = DamageOutput.PistolDamage(profile.Player.Stamina, profile.Player.Bleeding);
                string response;
                if (pistolDamage + trgt.Damage >= 100)
                {
                    DatabaseHungerGame.DieEvent(trgt.Userid);
                    response = $"Kills {trgt.Name} with his pistol inflicting {pistolDamage} damage.";
                }
                else
                {
                    DatabaseHungerGame.AddDamage(trgt.Userid, pistolDamage);
                    response = $"Hits {trgt.Name} with his pistol inflicting {pistolDamage} damage.";
                }
                return response;
            }
            
            if (profile.Weapons.Bow > 0)
            {
                var bowDamage = DamageOutput.BowDamage(profile.Player.Stamina, profile.Player.Bleeding);
                string response;
                if (bowDamage + trgt.Damage >= 100)
                {
                    DatabaseHungerGame.DieEvent(trgt.Userid);
                    response = $"Kills {trgt.Name} with his bow inflicting {bowDamage} damage.";
                }
                else
                {
                    DatabaseHungerGame.AddDamage(trgt.Userid, bowDamage);
                    response = $"Hits {trgt.Name} with his bow inflicting {bowDamage} damage.";
                }
                return response;
            }

            if (profile.Weapons.Axe > 0)
            {
                var axeDamage = DamageOutput.AxeDamage(profile.Player.Stamina, profile.Player.Bleeding);
                string response;
                if(axeDamage + trgt.Damage >= 100)
                {
                    DatabaseHungerGame.DieEvent(trgt.Userid);
                    response = $"Kills {trgt.Name} with his axe inflicting {axeDamage} damage.";
                }
                else
                {
                    DatabaseHungerGame.AddDamage(trgt.Userid, axeDamage);
                    response = $"Hits {trgt.Name} with his axe inflicting {axeDamage} damage.";
                }
                return response;
            }

            var fistDamage = DamageOutput.FistDamage(profile.Player.Stamina, profile.Player.Bleeding);
            string msg;
            if(fistDamage + trgt.Damage >= 100)
            {
                DatabaseHungerGame.DieEvent(trgt.Userid);
                msg = $"Kills {trgt.Name} with his fists inflicting {fistDamage} damage.";
            }
            else
            {
                DatabaseHungerGame.AddDamage(trgt.Userid, fistDamage);
                msg = $"Hits {trgt.Name} with his fists inflicting {fistDamage} damage.";
            }
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
                    Name = user.Name,
                    Damage = user.Damage
                }
            };
            return result;
        }
    }

    public class KillTarget
    {
        public string Name { get; set; }
        public ulong Userid { get; set; }
        public int Damage { get; set; }

        internal object ThrowIfNull()
        {
            throw new NotImplementedException();
        }
    }
}
