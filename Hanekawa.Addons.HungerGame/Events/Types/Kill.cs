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
                if (trgt.Health - pistolDamage <= 0)
                {
                    var user = db.HungerGameLives.Find(trgt.Userid);
                    user.Status = true;
                    user.Health = 0;
                    db.SaveChanges();
                    if (trgt.Userid == profile.UserId) response = "Commits sudoku";
                    else response = $"Kills {trgt.Name} with a pistol inflicting {pistolDamage} damage.";
                }
                else
                {
                    var user = db.HungerGameLives.Find(trgt.Userid);
                    user.Health = user.Health - pistolDamage;
                    db.SaveChanges();
                    if (trgt.Userid == profile.UserId) response = "Commits sudoku";
                    else response = $"Hits {trgt.Name} with a pistol inflicting {pistolDamage} damage.";
                }
                return response;
            }

            if (profile.Bow > 0)
            {
                var bowDamage = DamageOutput.BowDamage(profile.Stamina, profile.Bleeding);
                string response;
                if (trgt.Health - bowDamage <= 0)
                {
                    var user = db.HungerGameLives.Find(trgt.Userid);
                    user.Status = true;
                    user.Health = 0;
                    db.SaveChanges();
                    if (trgt.Userid == profile.UserId) response = "Commits sudoku";
                    else response = $"Kills {trgt.Name} with a bow inflicting {bowDamage} damage.";
                }
                else
                {
                    var user = db.HungerGameLives.Find(trgt.Userid);
                    user.Health = user.Health - bowDamage;
                    db.SaveChanges();
                    if (trgt.Userid == profile.UserId) response = "Commits sudoku";
                    else response = $"Hits {trgt.Name} with a bow inflicting {bowDamage} damage.";
                }
                return response;
            }

            if (profile.Axe > 0)
            {
                var axeDamage = DamageOutput.AxeDamage(profile.Stamina, profile.Bleeding);
                string response;
                if (trgt.Health - axeDamage <= 0)
                {
                    var user = db.HungerGameLives.Find(trgt.Userid);
                    user.Status = true;
                    user.Health = 0;
                    db.SaveChanges();
                    if (trgt.Userid == profile.UserId) response = "Commits sudoku";
                    else response = $"Kills {trgt.Name} with a axe inflicting {axeDamage} damage.";
                }
                else
                {
                    var user = db.HungerGameLives.Find(trgt.Userid);
                    user.Health = user.Health - axeDamage;
                    db.SaveChanges();
                    if (trgt.Userid == profile.UserId) response = "Commits sudoku";
                    else response = $"Hits {trgt.Name} with a axe inflicting {axeDamage} damage.";
                }
                return response;
            }

            var fistDamage = DamageOutput.FistDamage(profile.Stamina, profile.Bleeding);
            string msg;
            if (trgt.Health - fistDamage <= 0)
            {
                var user = db.HungerGameLives.Find(trgt.Userid);
                user.Status = true;
                user.Health = 0;
                db.SaveChanges();
                if (trgt.Userid == profile.UserId) msg = "Commits sudoku";
                msg = $"Kills {trgt.Name} with fists inflicting {fistDamage} damage.";
            }
            else
            {
                var user = db.HungerGameLives.Find(trgt.Userid);
                user.Health = user.Health - fistDamage;
                db.SaveChanges();
                if (trgt.Userid == profile.UserId) msg = "Commits sudoku";
                else msg = $"Hits {trgt.Name} with fists inflicting {fistDamage} damage.";
            }
            return msg;
        }

        private static KillTarget GetTarget(DbService db)
        {
            var users = db.HungerGameLives.Where(x => x.Status && x.Health > 0).ToList();
            var rand = new Random();
            var chosn = rand.Next(users.Count);
            var user = users[chosn];
            var result = new KillTarget
            {
                Userid = user.UserId,
                Name = user.Name,
                Health = user.Health
            };
            return result;
        }
    }

    public class KillTarget
    {
        public string Name { get; set; }
        public ulong Userid { get; set; }
        public int Health { get; set; }

        internal object ThrowIfNull()
        {
            throw new NotImplementedException();
        }
    }
}
