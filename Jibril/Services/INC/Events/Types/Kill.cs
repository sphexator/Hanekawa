using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Jibril.Services.INC.Calculate;
using Jibril.Services.INC.Data;

namespace Jibril.Services.INC.Events.Types
{
    public class Kill
    {
        public static string KillEvent(Profile profile, SocketGuildUser usr, SocketGuildUser trgt)
        {

            if (profile.Weapons.Pistol > 0 && profile.Weapons.Bullets > 0)
            {
                var pistolDamage = DamageOutput.PistolDamage(profile.Player.Stamina, profile.Player.Bleeding);
                var response = $"Hits {trgt.Username} with his axe inflicting {pistolDamage} damage.";
                return response;
            }

            if (profile.Weapons.Bow > 0 && profile.Weapons.Arrows > 0)
            {
                var bowDamage = DamageOutput.BowDamage(profile.Player.Stamina, profile.Player.Bleeding);
                var response = $"Hits {trgt.Username} with his axe inflicting {bowDamage} damage.";
                return response;
            }

            if (profile.Weapons.Axe > 0)
            {
                var axeDamage = DamageOutput.AxeDamage(profile.Player.Stamina, profile.Player.Bleeding);
                var response = $"Hits {trgt.Username} with his axe inflicting {axeDamage} damage.";
                return response;
            }

            var fistDamage = DamageOutput.FistDamage(profile.Player.Stamina, profile.Player.Bleeding);
            var msg = $"Hits {trgt.Username} with his axe inflicting {fistDamage} damage.";
            return msg;
        }
    }
}
