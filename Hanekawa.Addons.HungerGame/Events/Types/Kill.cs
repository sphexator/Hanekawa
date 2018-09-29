using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables;
using Hanekawa.Addons.HungerGame.Calculate;
using System;
using System.Linq;

namespace Hanekawa.Addons.HungerGame.Events.Types
{
    public class Kill
    {
        public static string KillEvent(HungerGameLive profile, DbService db)
        {
            var trgt = GetTarget(db);
            if (profile.Pistol > 0)
            {
                var pistolDamage = DamageOutput.PistolDamage(profile.Stamina, profile.Bleeding);
                string response;
                if (pistolDamage + trgt.Damage >= 100)
                {
                    var user = db.HungerGameLives.Find(trgt.Userid);
                    user.Status = true;
                    user.Health = 0;
                    db.SaveChanges();
                    response = $"Kills {trgt.Name} with a pistol inflicting {pistolDamage} damage.";
                }
                else
                {
                    var user = db.HungerGameLives.Find(trgt.Userid);
                    user.Status = false;
                    user.Health = 0;
                    db.SaveChanges();
                    response = $"Hits {trgt.Name} with a pistol inflicting {pistolDamage} damage.";
                }
                return response;
            }

            if (profile.Bow > 0)
            {
                var bowDamage = DamageOutput.BowDamage(profile.Stamina, profile.Bleeding);
                string response;
                if (bowDamage + trgt.Damage >= 100)
                {
                    var user = db.HungerGameLives.Find(trgt.Userid);
                    user.Status = true;
                    user.Health = 0;
                    db.SaveChanges();
                    response = $"Kills {trgt.Name} with a bow inflicting {bowDamage} damage.";
                }
                else
                {
                    var user = db.HungerGameLives.Find(trgt.Userid);
                    user.Status = false;
                    user.Health = 0;
                    db.SaveChanges();
                    response = $"Hits {trgt.Name} with a bow inflicting {bowDamage} damage.";
                }
                return response;
            }

            if (profile.Axe > 0)
            {
                var axeDamage = DamageOutput.AxeDamage(profile.Stamina, profile.Bleeding);
                string response;
                if (axeDamage + trgt.Damage >= 100)
                {
                    var user = db.HungerGameLives.Find(trgt.Userid);
                    user.Status = true;
                    user.Health = 0;
                    db.SaveChanges();
                    response = $"Kills {trgt.Name} with a axe inflicting {axeDamage} damage.";
                }
                else
                {
                    var user = db.HungerGameLives.Find(trgt.Userid);
                    user.Status = false;
                    user.Health = 0;
                    db.SaveChanges();
                    response = $"Hits {trgt.Name} with a axe inflicting {axeDamage} damage.";
                }
                return response;
            }

            var fistDamage = DamageOutput.FistDamage(profile.Stamina, profile.Bleeding);
            string msg;
            if (fistDamage + trgt.Damage >= 100)
            {
                var user = db.HungerGameLives.Find(trgt.Userid);
                user.Status = true;
                user.Health = 0;
                db.SaveChanges();
                msg = $"Kills {trgt.Name} with fists inflicting {fistDamage} damage.";
            }
            else
            {
                var user = db.HungerGameLives.Find(trgt.Userid);
                user.Status = false;
                user.Health = 0;
                db.SaveChanges();
                msg = $"Hits {trgt.Name} with fists inflicting {fistDamage} damage.";
            }
            return msg;
        }

        private static KillTarget GetTarget(DbService db)
        {
            var users = db.HungerGameLives.ToList();
            var rand = new Random();
            var chosn = rand.Next(users.Count);
            var user = users[chosn];
            var result = new KillTarget
            {
                Userid = user.UserId,
                Name = user.Name,
                Damage = user.Health
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
