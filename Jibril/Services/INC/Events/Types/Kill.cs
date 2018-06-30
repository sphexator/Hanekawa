using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Discord.WebSocket;
using Google.Apis.Util;
using Jibril.Services.Entities;
using Jibril.Services.Entities.Tables;
using Jibril.Services.INC.Calculate;
using Jibril.Services.INC.Data;
using Jibril.Services.INC.Database;

namespace Jibril.Services.INC.Events.Types
{
    public static class Kill
    {
        public static string KillEvent(HungerGameLive profile)
        {
            using (var db = new DbService())
            {
                var trgt = GetTarget().FirstOrDefault();
                if (profile.Pistol> 0)
                {
                    var pistolDamage = DamageOutput.PistolDamage(profile.Stamina, profile.Bleeding);
                    string response;
                    if (pistolDamage + trgt.Damage >= 100)
                    {
                        var user = db.HungerGameLives.Find(trgt.Userid.ToString());
                        user.Status = true;
                        user.Health = 0;
                        db.SaveChanges();
                        response = $"Kills {trgt.Name} with his pistol inflicting {pistolDamage} damage.";
                    }
                    else
                    {
                        var user = db.HungerGameLives.Find(trgt.Userid.ToString());
                        user.Status = false;
                        user.Health = 0;
                        db.SaveChanges();
                        response = $"Hits {trgt.Name} with his pistol inflicting {pistolDamage} damage.";
                    }
                    return response;
                }

                if (profile.Bow > 0)
                {
                    var bowDamage = DamageOutput.BowDamage(profile.Stamina, profile.Bleeding);
                    string response;
                    if (bowDamage + trgt.Damage >= 100)
                    {
                        var user = db.HungerGameLives.Find(trgt.Userid.ToString());
                        user.Status = true;
                        user.Health = 0;
                        db.SaveChanges();
                        response = $"Kills {trgt.Name} with his bow inflicting {bowDamage} damage.";
                    }
                    else
                    {
                        var user = db.HungerGameLives.Find(trgt.Userid.ToString());
                        user.Status = false;
                        user.Health = 0;
                        db.SaveChanges();
                        response = $"Hits {trgt.Name} with his bow inflicting {bowDamage} damage.";
                    }
                    return response;
                }

                if (profile.Axe > 0)
                {
                    var axeDamage = DamageOutput.AxeDamage(profile.Stamina, profile.Bleeding);
                    string response;
                    if (axeDamage + trgt.Damage >= 100)
                    {
                        var user = db.HungerGameLives.Find(trgt.Userid.ToString());
                        user.Status = true;
                        user.Health = 0;
                        db.SaveChanges();
                        response = $"Kills {trgt.Name} with his axe inflicting {axeDamage} damage.";
                    }
                    else
                    {
                        var user = db.HungerGameLives.Find(trgt.Userid.ToString());
                        user.Status = false;
                        user.Health = 0;
                        db.SaveChanges();
                        response = $"Hits {trgt.Name} with his axe inflicting {axeDamage} damage.";
                    }
                    return response;
                }

                var fistDamage = DamageOutput.FistDamage(profile.Stamina, profile.Bleeding);
                string msg;
                if (fistDamage + trgt.Damage >= 100)
                {
                    var user = db.HungerGameLives.Find(trgt.Userid.ToString());
                    user.Status = true;
                    user.Health = 0;
                    db.SaveChanges();
                    msg = $"Kills {trgt.Name} with his fists inflicting {fistDamage} damage.";
                }
                else
                {
                    var user = db.HungerGameLives.Find(trgt.Userid.ToString());
                    user.Status = false;
                    user.Health = 0;
                    db.SaveChanges();
                    msg = $"Hits {trgt.Name} with his fists inflicting {fistDamage} damage.";
                }
                return msg;
            }
        }

        private static IEnumerable<KillTarget> GetTarget()
        {
            using (var db = new DbService())
            {
                var users = db.HungerGameLives.ToList();
                var rand = new Random();
                var chosn = rand.Next(users.Count);
                var user = users[chosn];
                var result = new List<KillTarget>
                {
                    new KillTarget
                    {
                        Userid = user.UserId,
                        Name = user.Name,
                        Damage = user.Health
                    }
                };
                return result;
            }
        }
    }

    public class KillTarget
    {
        public string Name { get; set; }
        public ulong Userid { get; set; }
        public uint Damage { get; set; }

        internal object ThrowIfNull()
        {
            throw new NotImplementedException();
        }
    }
}
